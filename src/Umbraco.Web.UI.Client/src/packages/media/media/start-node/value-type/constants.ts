export const UMB_MEDIA_START_NODE_VALUE_TYPE = 'Umb.ValueType.Media.StartNode' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MEDIA_START_NODE_VALUE_TYPE]: { unique: string } | null;
	}
}
