export type UmbApiConstructorArgumentsMethodType<ManifestType, ReturnType extends Array<unknown> = Array<unknown>> = (
	manifest: ManifestType,
) => ReturnType;
