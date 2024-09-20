import type { ManifestTiptapExtension } from './tiptap-extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbTiptapExtensionApi extends UmbApi {
	getTiptapExtensions(): Array<Extension | Mark | Node>;
}

export abstract class UmbTiptapExtensionApiBase extends UmbControllerBase implements UmbApi {
	public manifest?: ManifestTiptapExtension;

	constructor(host: UmbControllerHost) {
		super(host);
	}

	abstract getTiptapExtensions(): Array<Extension | Mark | Node>;
}

export interface UmbTiptapToolbarElementApi extends UmbTiptapExtensionApi {
	execute(editor?: Editor): void;
	isActive(editor?: Editor): boolean;
}

export abstract class UmbTiptapToolbarElementApiBase
	extends UmbTiptapExtensionApiBase
	implements UmbTiptapToolbarElementApi
{
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public execute(editor?: Editor) {}

	public isActive(editor?: Editor) {
		return editor && this.manifest?.meta.alias ? editor?.isActive(this.manifest.meta.alias) : false;
	}
}
