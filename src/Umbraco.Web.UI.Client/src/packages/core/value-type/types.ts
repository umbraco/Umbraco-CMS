// Base declaration — packages extend via declaration merging
declare global {
	interface UmbValueTypeMap {}
}

/** Union of all registered value type keys. */
export type UmbValueType = keyof UmbValueTypeMap;
