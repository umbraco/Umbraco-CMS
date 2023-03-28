export * from './utils';
export * from './umbraco-path';
export * from './udi-service';
export * from './media-helper.service';

declare global {
	interface Window {
		tinymce: any;
		Umbraco: any;
	}
}