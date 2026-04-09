export const UMB_DOCUMENT_START_NODE_VALUE_TYPE = 'Umb.ValueType.Document.StartNode' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_DOCUMENT_START_NODE_VALUE_TYPE]: { unique: string } | null;
	}
}
