---
name: appsec-remediation
description: Invoke for security code reviews, OWASP/CWE vulnerability fixes, and defensive refactoring of C#/.NET or T-SQL (SQLi, XSS, IDOR, path traversal, sensitive logging, missing validation). Traces taint, applies a minimal fix, adds xUnit exploit tests, and ends with a human review checklist.
---
# AppSec Remediation Workflow
Run these 4 steps in order. Be terse. Never skip step 3 or step 4.

## 1. Trace Taint & Classify
- Identify the **source** (query/route/body/header, file, DB field) and the **sink** (SQL, HTML/JS, file path, logger, authorization lookup).
- Output one line per finding: `source -> sink | CWE-<id> | OWASP A0x | severity`.
- If there is no exploitable path, say so and stop.

## 2. Minimal Defensive Fix
- Fix at the sink: parameterize, encode, canonicalize or authorize. Add allowlist validation at the boundary.
- Preserve signatures, return types, status codes and data contracts.
- Touch the fewest lines possible. No unrelated refactors.
- Where business intent is ambiguous, insert `// HUMAN-REVIEW: <question>` instead of guessing.

## 3. xUnit Regression & Exploit-Attempt Tests
- Write 1 happy-path test proving existing behavior is unchanged.
- Write `[Theory]` exploit tests using real payloads for the CWE, for example:
  - `' OR 1=1--` and `'; DROP TABLE Users--`
  - `<script>alert(1)</script>`
  - `..\..\web.config`
  - another user's or tenant's ID
- Add edge cases: null, empty, whitespace and max length.
- Tests must fail against the vulnerable code and pass against the fix.

## 4. Human Review Checklist (exactly 3 bullets)
```
### Human Review Checklist
- **Upstream:** <assumption about callers, input sources, auth context>
- **Downstream:** <assumption about DB schema, consumers, encoding, contracts>
- **Residual risk:** <what this fix does NOT cover; what to verify manually>
```