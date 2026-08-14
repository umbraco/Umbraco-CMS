export const UMB_MEDIA_USER_START_NODE_VALUE_TYPE = 'Umb.ValueType.Media.UserStartNode' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_MEDIA_USER_START_NODE_VALUE_TYPE]: { unique: string } | null;
	}
}
