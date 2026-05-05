export const UMB_DOCUMENT_USER_START_NODE_VALUE_TYPE = 'Umb.ValueType.Document.UserStartNode' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DOCUMENT_USER_START_NODE_VALUE_TYPE]: { unique: string } | null;
	}
}
