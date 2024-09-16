import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbTiptapExtensionBase extends UmbControllerBase implements UmbApi {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	abstract getExtensions(): Array<Extension | Mark | Node>;

	abstract getToolbarButtons(): Array<UmbTiptapToolbarButton>;
}

export interface UmbTiptapToolbarButton {
	name: string;
	icon: string | TemplateResult;
	isActive: (editor?: Editor) => boolean | undefined;
	command: (editor?: Editor) => boolean | undefined | void | Promise<boolean> | Promise<undefined> | Promise<void>;
}
