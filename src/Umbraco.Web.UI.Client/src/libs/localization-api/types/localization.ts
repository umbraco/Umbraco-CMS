// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type UmbLocalizationEntry = string | ((...args: any[]) => string);
export type UmbLocalizationDictionary = Record<string, Record<string, UmbLocalizationEntry>>;
export type UmbLocalizationFlatDictionary = Record<string, UmbLocalizationEntry>;
