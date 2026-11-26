# MYT-3108 notes

## What the agent proposed

The agent bound the search text as a parameter, then stripped quotes and semicolons from `sortBy` and still concatenated it into `ORDER BY`. It also deleted apostrophes from the search text and switched `LIKE` to an exact match. One of its tests, `Search_ExactMatchOnly_BlocksWildcardEnumeration`, asserted that `dental` returns nothing.

## What I changed and why

`sortBy` and `sortDir` are allowlisted (`date`, `amount`, `status` and `asc`, `desc`); anything else throws `ArgumentException`. The search text is still a parameter, apostrophes are kept, and the pattern is a contains-match with `%` and `_` escaped so `O'Brien` and `100%` match only the real text. The trap test now expects the partial match.

## Evidence

`dotnet test tests/Claims.Tests` passes, including partial search, `O'Brien`, literal `%`, other-member isolation, and the sort allowlist payloads.
