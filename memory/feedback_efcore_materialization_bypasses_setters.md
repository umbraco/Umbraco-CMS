---
name: efcore-materialization-bypasses-setters
description: EF Core writes backing fields directly when materialising, so any EF DTO setter with logic (e.g. decimal Normalize) silently never runs; configure UsePropertyAccessMode(Property) or move the logic to the getter
metadata:
  type: feedback
---

Found 2026-09-25: PropertyDataDto.DecimalValue's setter normalises trailing zeros, but EF Core's default PreferField access wrote `_decimalValue` directly, so SQL Server's decimal(20,9) came back as 12.455200000 and the search index tests failed on SQL Server only (SQLite has no fixed scale, so it never showed locally).

**Why:** NPoco assigns through property setters; EF Core does not. A DTO ported from NPoco keeps looking correct while its setter is dead on the read path.

**How to apply:** when porting a NPoco DTO, grep it for `set =>` bodies; for each one that changes the value, add `.UsePropertyAccessMode(PropertyAccessMode.Property)` in the configuration (or normalise in the getter). Reproduce on SQLite by inserting the padded TEXT value through raw SQL, as PropertyDataDtoMaterializationTests does. Related: [[efcore-notracking-requires-astracking]].
