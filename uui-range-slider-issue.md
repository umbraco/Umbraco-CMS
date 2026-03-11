# `uui-range-slider`: `minGap=0` treated as unset, preventing overlapping handles

## Bug summary

When `minGap` is explicitly set to `0` on `uui-range-slider`, the component ignores it and falls back to using `step` as the minimum gap between handles. This prevents both handles from selecting the same value (e.g. a range of "6 to 6"), which is a valid use case for range sliders representing filters like age ranges or price ranges.

## Reproduction

```html
<uui-range-slider label="Test" min="0" max="10" step="1" .minGap=${0}></uui-range-slider>
```

Try to drag both handles to the same value — the component enforces a minimum gap of `1` (the step value).

## Expected behavior

Setting `minGap=0` should explicitly allow both handles to overlap, selecting equal low and high values.

## Root cause

The component uses JavaScript truthiness checks to determine whether `minGap` was configured. Since `0` is falsy, it's indistinguishable from `undefined` (not set).

**Affected locations in `range-slider.element.ts`:**

**1. `_runGapChecks()`** — clears `_minGap` when it's less than `step`, but the guard `this._minGap &&` already skips `0`:
```ts
if (this._minGap && this._minGap < this._step) {
    this._minGap = undefined;
```

**2. `setValueLow()` / `setValueHigh()`** — falls back to `step` gap when `minGap` is falsy:
```ts
const clampMax = this.minGap
    ? this._highInputValue - this.minGap
    : this._highInputValue - this.step;
```

**3. `#transferValueToInternalValues()`** — same pattern, two occurrences:
```ts
this._minGap ? high - this._minGap : high - this._step
```

**4. `_runPropertiesChecks()`** — logs an error when both values are equal, even when that's intentionally allowed:
```ts
if (this._highInputValue === this._lowInputValue) {
    console.error(`...Low-end and high-end value should never be equal...`);
}
```

## Suggested fix

Replace truthiness checks with `!== undefined` so that `0` is treated as an explicit value:

**`_runGapChecks()`:**
```ts
// Before:
if (this._minGap && this._minGap < this._step) {
// After:
if (this._minGap !== undefined && this._minGap > 0 && this._minGap < this._step) {
```

**`setValueLow()`:**
```ts
// Before:
const clampMax = this.minGap
    ? this._highInputValue - this.minGap
    : this._highInputValue - this.step;
// After:
const clampMax = this.minGap !== undefined
    ? this._highInputValue - this.minGap
    : this._highInputValue - this.step;
```

**`setValueHigh()`:**
```ts
// Before:
const clampMin = this.minGap
    ? this._lowInputValue + this.minGap
    : this._lowInputValue + this.step;
// After:
const clampMin = this.minGap !== undefined
    ? this._lowInputValue + this.minGap
    : this._lowInputValue + this.step;
```

**`#transferValueToInternalValues()`** (two occurrences):
```ts
// Before:
this._minGap ? high - this._minGap : high - this._step
// After:
this._minGap !== undefined ? high - this._minGap : high - this._step
```

```ts
// Before:
this._minGap
    ? this._lowInputValue + this._minGap
    : this._lowInputValue + this._step
// After:
this._minGap !== undefined
    ? this._lowInputValue + this._minGap
    : this._lowInputValue + this._step
```

**`_runPropertiesChecks()`:**
```ts
// Before:
if (this._highInputValue === this._lowInputValue) {
// After:
if (this._highInputValue === this._lowInputValue && this._minGap !== 0) {
```

## Context

This is blocking the Umbraco CMS slider from supporting zero-gap ranges. See [Umbraco-CMS#22067](https://github.com/umbraco/Umbraco-CMS/issues/22067) and [Umbraco-CMS#22078](https://github.com/umbraco/Umbraco-CMS/pull/22078). The CMS-side changes pass `minGap` through correctly and are ready — they'll work as soon as this is fixed.
