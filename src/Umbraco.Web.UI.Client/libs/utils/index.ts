export * from './umbraco-path';
export * from './udi-service';
export * from './media-helper.service';

// TODO => tinymce property should be typed, but would require libs taking a dependency on TinyMCE, which is not ideal
declare global {
	interface Window {
		tinymce: any;
		Umbraco: any;
	}
}