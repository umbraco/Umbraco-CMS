---
name: project_search_decimal_tests_fail_on_comma_locale
description: On this machine (Windows user locale en-DK, decimal separator comma) three VariantDocumentTests decimal cases fail on every branch; it is Examine formatting a stored double with the current culture, not a code regression
metadata:
  type: project
---

`VariantDocumentTests.CanIndexInvariantProperty("invariantdecimalproperty", "decimals", 12.4552, <culture>)` fails
for all three cultures on this machine, on every branch (verified 12-10-2026 on `95111fe68ac` and on PR 1's tip
`aa952e94477`), with `Sequence contains no matching element`: the stored field reads back as `"12,4552"` while the
test compares against the invariant `"12.4552"`.

**Why:** Examine 4.0.0's `LuceneSearchExecutor` calls the parameterless `Field.GetStringValue()`, which formats the
double through `NumberFormatInfo.CurrentInfo`; the machine's user locale is `en-DK` (`HKCU\Control Panel\International\LocaleName`),
whose decimal separator is a comma. Nothing in the harness pins a culture, and `[SetCulture]` on the fixture is not
enough because the formatting happens off the test thread; setting `CultureInfo.DefaultThreadCurrentCulture`
makes all nine cases pass. `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` is not a workaround (the fixture needs real cultures).
CI runs on Linux/macOS and does not hit it.

**How to apply:** when a local search-fixture run shows exactly these three decimal cases on top of the 23
relation-stub baseline ([[project_ef_core_element_repository_campaign]] §4a), it is not a regression. The fix, if
wanted, is a test hardening (compare numerically, or pin `DefaultThreadCurrentCulture` in the Examine
`IndexTestBase`); the culture-sensitive call itself is upstream in Examine.
