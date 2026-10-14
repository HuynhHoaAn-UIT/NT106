# Task 01 — Socket Protocol Design

**Repository:** HuynhHoaAn-UIT/NT106  
**Branch:** luuchibao7031  
**Status:** Protocol proposal for the next implementation tasks

## 1. Repository review

The selected branch currently contains a C# WinForms starter: LoginForm, RegisterForm, OTPForm and MainForm. Program.cs starts LoginForm. The forms currently show local success messages; no TCP client/server or database calls are present. The project targets .NET 10 Windows Forms.

This document defines the proposed wire contract; it does not implement the server.

## 2. Requirements inferred from the project plan

The protocol must support TCP connections, registration and OTP verification, login/logout, heartbeat, scholarship chat and broadcast, scholarship listing/following, posts/upvotes, leaderboard queries and essay/file transfer. It must support multiple concurrent clients, detect malformed messages, avoid blocking the UI, and return errors in a consistent format.

TCP is a byte stream. A single read may contain part of a frame or multiple frames. Both sides must read exact lengths and must not assume one Receive equals one message.

## 3. Frame format

Each frame has a fixed 20-byte header followed by BodyLength bytes. All multi-byte integers use network byte order (big-endian).

| Offset | Bytes | Field | Definition |
|---:|---:|---|---|
| 0 | 4 | Magic | ASCII SGW1 |
| 4 | 1 | Version | 1 |
| 5 | 1 | Flags | bit 0=response, bit 1=error, bit 2=encrypted, bit 3=binary |
| 6 | 2 | MessageType | unsigned message code |
| 8 | 4 | BodyLength | body byte count |
| 12 | 8 | RequestId | request/response correlation ID |

BodyLength counts bytes, not characters. No newline or terminator is used. The receiver reads exactly 20 header bytes, validates them, then reads exactly BodyLength bytes.

Limits: JSON body at most 1 MiB; binary file chunk at most 64 KiB. Reject invalid magic/version, unknown mandatory message type, unsupported flags, or oversized body before allocating the body buffer. Close the connection after a framing violation.

## 4. Body format

Unless marked binary, body is UTF-8 JSON using camelCase. IDs are strings; timestamps are UTC ISO 8601.

Request example:

```json
{
  "type": "LOGIN",
  "data": { "email": "student@example.edu", "password": "..." }
}
```

Success response example:

```json
{
  "type": "LOGIN",
  "data": { "userId": "usr_123", "displayName": "Nguyen Van A", "token": "..." }
}
```

Error response example:

```json
{
  "type": "ERROR",
  "data": {
    "code": "AUTH_INVALID_CREDENTIALS",
    "message": "Email or password is incorrect.",
    "retryable": false
  }
}
```

Responses set the response flag and reuse the request's RequestId. Error responses set both response and error flags. Server-initiated messages use RequestId 0. A RequestId is unique among in-flight requests on a connection.

## 5. Proposed MessageType registry (Protocol v1)

| Code | Name | Direction | Body summary |
|---:|---|---|---|
| 0x0001 | HELLO | Client → Server | protocolVersion, clientName |
| 0x0002 | HELLO_ACK | Server → Client | protocolVersion, serverTime |
| 0x0003 | ERROR | Either | code, message, retryable |
| 0x0010 | REGISTER | Client → Server | name, email, password |
| 0x0011 | REGISTER_OTP_VERIFY | Client → Server | email, otp |
| 0x0012 | REGISTER_OTP_RESEND | Client → Server | email |
| 0x0013 | LOGIN | Client → Server | email, password |
| 0x0014 | LOGOUT | Client → Server | token |
| 0x0020 / 0x0021 | PING / PONG | Client ↔ Server | empty body or timestamp |
| 0x0030 | SCHOLARSHIP_LIST | Client → Server | filters, page, pageSize |
| 0x0031 | SCHOLARSHIP_LIST_RESULT | Server → Client | items, page, total |
| 0x0032 | SCHOLARSHIP_FOLLOW | Client → Server | scholarshipId |
| 0x0040 | CHAT_SEND | Client → Server | scholarshipId, text, clientMessageId |
| 0x0041 | CHAT_MESSAGE | Server → room members | messageId, scholarshipId, sender, text, sentAt |
| 0x0042 / 0x0043 | CHAT_HISTORY / RESULT | Client ↔ Server | scholarshipId, cursor, items |
| 0x0050 | POST_CREATE | Client → Server | title, content |
| 0x0051 | POST_VOTE | Client → Server | postId, vote |
| 0x0060 / 0x0061 | LEADERBOARD_TOP / RESULT | Client ↔ Server | scholarshipId, limit / ranked items |
| 0x0062 / 0x0063 | LEADERBOARD_POSITION / RESULT | Client ↔ Server | scholarshipId / rank, totalScore, deltaXp |
| 0x0070 | FILE_UPLOAD_START | Client → Server | fileName, fileSize, sha256, relatedId |
| 0x0071 | FILE_CHUNK | Client → Server | binary chunk; chunk metadata format to be agreed |
| 0x0072 / 0x0073 | FILE_UPLOAD_COMPLETE / RESULT | Client ↔ Server | uploadId / status |
| 0x0080 | SESSION_REVOKED | Server → Client | reason |

This registry is a proposed contract based on the task list. Feature owners must confirm payload fields before implementation. Do not reuse a code within Protocol v1 once clients depend on it.

## 6. Connection and session flow

1. Client opens the configured TCP connection and sends HELLO (header version 1).
2. Server returns HELLO_ACK. Unsupported versions receive ERROR(PROTOCOL_VERSION_UNSUPPORTED), then disconnect.
3. Client sends LOGIN, or REGISTER followed by OTP verification.
4. After successful login, the client sends PING every 15 seconds and expects PONG.
5. On logout, session revocation, timeout, or protocol error, stop heartbeat and close resources. Reconnection uses a bounded backoff; authenticate again if the session is no longer valid.

Server authorization must validate that the user may access the supplied Scholarship_ID. Never trust client-provided IDs as proof of access.

## 7. AES and file transfer constraints

Task 26 mentions AES. Do not hardcode a shared AES key in source. The team must decide secure key provisioning before enabling encrypted frames. Prefer AES-GCM with a versioned envelope containing nonce, authentication tag and ciphertext. Keep framing and correlation metadata readable. TLS is still recommended for network transport; application AES does not replace TLS or server authentication.

For files, send metadata with FILE_UPLOAD_START, binary chunks no larger than 64 KiB, then FILE_UPLOAD_COMPLETE. Server verifies total size and SHA-256 before success. Decide whether chunk metadata includes uploadId, chunkIndex and offset, and whether resume is required, before Task 19. Do not base64-encode large files inside JSON.

## 8. Parser and implementation rules

- Read exactly 20 header bytes and then exactly BodyLength body bytes; handle short reads and coalesced frames.
- Validate length and flags before allocating memory.
- Use a per-connection send queue so concurrent writes cannot interleave frame bytes.
- Apply bounded queues and timeouts so a slow client cannot consume unbounded memory.
- Do not log passwords, OTP values, tokens, encryption keys or file contents.
- Marshal WinForms updates to the UI thread with Invoke/BeginInvoke.
- Cancel read/write/heartbeat tasks and dispose the socket and buffers on disconnect.
- Return stable error codes; never expose stack traces to the client.

## 9. Decisions the team must confirm

1. Server host/port for development and demo.
2. Whether authentication state is a token or socket-bound session, and token expiry/refresh.
3. OTP expiry, retry limit and resend cooldown.
4. How AES keys are provisioned.
5. Whether chat supports attachments.
6. Whether file uploads can resume after reconnect.
7. Whether scholarship, post, profile and support operations use this TCP protocol or a separate HTTP API.
8. Whether leaderboard ranking is scoped per scholarship or global.

These decisions do not block implementing a basic frame parser, but each must be settled before its corresponding business message is finalized.

## 10. Task 01 completion criteria

- Header/body framing, byte order, limits and error behavior are specified.
- Client and server developers can implement the same parser without guessing.
- Request correlation, server-push messages and heartbeat are defined.
- Open protocol decisions are recorded for the team.
- No server implementation or full database schema is included in Task 01.
