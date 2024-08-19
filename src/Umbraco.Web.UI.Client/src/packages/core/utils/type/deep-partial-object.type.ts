/*export type DeepPartial<T> = T extends { [key: string]: any }
	? {
			[P in keyof T]?: DeepPartial<T[P]>;
		}
	: T;
*/

// Notice this can be way more complex, but in this case I just wanted to cover pure objects, to match our deep merge function [NL]
// See https://stackoverflow.com/questions/61132262/typescript-deep-partial for more extensive solutions.
/**
 * Deep partial object type, making objects and their properties optional, but only until a property of a different type is encountered.
 * This means if an object holds a property with an array that holds objects, the array will be made optional, but the properties of the objects inside the array will not be changed.
 * @type UmbDeepPartialObject
 * @generic T - The object to make partial.
 * @returns A type with all properties of objects made optional.
 */
// eslint-disable-next-line @typescript-eslint/no-unsafe-function-type
export type UmbDeepPartialObject<T> = T extends Function
	? T
	: // Thing extends Array<infer InferredArrayMember>
		// ? DeepPartialArray<InferredArrayMember> :
		T extends { [key: string]: any }
		? UmbDeepPartialObjectProperty<T>
		: T | undefined;

//interface DeepPartialArray<Thing> extends Array<DeepPartial<Thing>> {}

type UmbDeepPartialObjectProperty<Thing> = {
	[Key in keyof Thing]?: UmbDeepPartialObject<Thing[Key]>;
};
