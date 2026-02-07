class u {
  async processValue(a, s, r, t) {
    var e = "Preset value";
    return r?.variesByCulture && (e += " varies by culture: " + t?.variantId?.culture), r?.variesBySegment && (e += " varies by segment: " + (t?.variantId?.segment ?? "default")), a || e;
  }
  destroy() {
  }
}
export {
  u as MyPropertyValuePresetApi,
  u as api
};
//# sourceMappingURL=property-preset-value.js.map
