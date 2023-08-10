// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type UmbTranslationEntry = string | ((...args: any[]) => string);
export type UmbTranslationsDictionary = Record<string, Record<string, UmbTranslationEntry>>;
export type UmbTranslationsFlatDictionary = Record<string, UmbTranslationEntry>;
