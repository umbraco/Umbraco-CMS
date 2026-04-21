declare global {
	// eslint-disable-next-line @typescript-eslint/no-empty-object-type -- intentionally empty; packages extend via declaration merging
	interface UmbValueTypeMap {}
}

/** Union of all registered value type keys. */
export type UmbValueType = keyof UmbValueTypeMap;
