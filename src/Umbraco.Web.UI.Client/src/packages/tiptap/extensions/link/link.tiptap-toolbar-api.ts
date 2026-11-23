import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { UmbLink } from './link.tiptap-extension.js';
import { attributesFromLink, linkFromAttributes } from './link-attributes.function.js';
import { UMB_LINK_PICKER_MODAL } from '@umbraco-cms/backoffice/multi-url-picker';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

export default class UmbTiptapToolbarLinkExtensionApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		const attrs = editor?.getAttributes(UmbLink.name) ?? {};
		const link = linkFromAttributes(attrs);
		const data = { config: { disableNameAutofill: true }, index: null, isNew: link?.url === undefined };
		const value = { link };

		const overlaySize = this.configuration?.getValueByAlias<UUIModalSidebarSize>('overlaySize') ?? 'small';

		const result = await umbOpenModal(this, UMB_LINK_PICKER_MODAL, { data, value, modal: { size: overlaySize } }).catch(
			() => undefined,
		);
		if (!result?.link) return;

		const linkAttrs = attributesFromLink(result.link);

		if (linkAttrs) {
			editor?.chain().focus().extendMarkRange(UmbLink.name).setUmbLink(linkAttrs).run();
		} else {
			editor?.chain().focus().extendMarkRange(UmbLink.name).unsetLink().run();
		}
	}
}
