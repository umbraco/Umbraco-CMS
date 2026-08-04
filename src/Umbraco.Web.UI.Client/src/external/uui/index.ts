import '@umbraco-ui/uui';
export * from '@umbraco-ui/uui';

// UUI 2.0 removed the global ambient `Option` interface (its replacement is the exported
// `UUISelectOption`). Re-declare it here so existing backoffice code — and third-party
// extensions that referenced the ambient `Option` type — continue to compile unchanged.
// TODO (V19): migrate consumers to `UUISelectOption` and remove this compatibility shim.
declare global {
	interface Option {
		name: string;
		value: string;
		group?: string;
		selected?: boolean;
		disabled?: boolean;
	}
}
