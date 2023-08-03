export type UmbTranslationEntry = string | ((...args: never[]) => string);
export type UmbTranslationsDictionary = Record<string, Record<string, UmbTranslationEntry>>;
export type UmbTranslationsFlatDictionary = Record<string, UmbTranslationEntry>;
