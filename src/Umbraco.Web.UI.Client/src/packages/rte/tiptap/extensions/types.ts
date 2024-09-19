import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export abstract class UmbTiptapExtensionApi extends UmbControllerBase implements UmbApi {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	execute(editor?: Editor) {}

	abstract getTiptapExtensions(args?: UmbTiptapExtensionArgs): Array<Extension | Mark | Node>;
}

export interface UmbTiptapExtensionArgs {
	/**
	 * The data type configuration for the property editor that the editor is used for.
	 * You can populate this manually if you are using the editor outside of a property editor with the {@link UmbPropertyEditorConfigCollection} object.
	 * @remark This is only available when the editor is used in a property editor or populated manually.
	 */
	configuration?: UmbPropertyEditorConfigCollection;
}

export interface UmbTiptapToolbarButton {
	name: string;
	icon: string | TemplateResult;
	isActive: (editor?: Editor) => boolean | undefined;
	command: (editor?: Editor) => boolean | undefined | void | Promise<boolean> | Promise<undefined> | Promise<void>;
}
