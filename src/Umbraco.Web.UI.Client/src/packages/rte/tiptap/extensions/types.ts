import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export abstract class UmbTiptapExtensionApi extends UmbControllerBase implements UmbApi {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	execute(editor?: Editor) {}

	abstract getTiptapExtensions(): Array<Extension | Mark | Node>;
}

export interface UmbTiptapToolbarButton {
	name: string;
	icon: string | TemplateResult;
	isActive: (editor?: Editor) => boolean | undefined;
	command: (editor?: Editor) => boolean | undefined | void | Promise<boolean> | Promise<undefined> | Promise<void>;
}
