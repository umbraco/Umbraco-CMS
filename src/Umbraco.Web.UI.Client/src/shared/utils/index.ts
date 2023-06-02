export * from './generate-umbraco-alias.function.js';
export * from './umbraco-path.js';
export * from './udi-service.js';
export * from './selection-manager.js';
export * from './media-helper.service';

// TODO => tinymce property should be typed, but would require libs taking a dependency on TinyMCE, which is not ideal
declare global {
	interface Window {
		tinymce: any;
		Umbraco: any;
	}
}