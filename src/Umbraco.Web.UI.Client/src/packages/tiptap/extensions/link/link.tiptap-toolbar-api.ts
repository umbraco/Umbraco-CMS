import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { UmbLink } from './link.tiptap-extension.js';
import { attributesFromLink, linkFromAttributes } from './link-attributes.function.js';
import { UMB_LINK_PICKER_MODAL } from '@umbraco-cms/backoffice/multi-url-picker';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { UmbLinkPickerLink } from '@umbraco-cms/backoffice/multi-url-picker';
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

		this.#applyLink(result.link, editor);
	}

	#applyLink(link: UmbLinkPickerLink, editor?: Editor) {
		const linkAttrs = attributesFromLink(link);

		if (!linkAttrs) {
			editor?.chain().focus().extendMarkRange(UmbLink.name).unsetLink().run();
		} else if (this.#hasTextToLink(editor)) {
			editor?.chain().focus().extendMarkRange(UmbLink.name).setUmbLink(linkAttrs).run();
		} else {
			// Insert the link instead, using its name as the text and falling back to the picked URL, as a
			// local link's href is a `{localLink:…}` token. Storing the link mark before inserting plain text
			// keeps any formatting at the cursor.
			const text = link.name || link.url || linkAttrs.href;
			editor?.chain().focus().setUmbLink(linkAttrs).insertContent({ type: 'text', text }).unsetMark(UmbLink.name).run();
		}
	}

	/**
	 * Whether there is text for the link mark to apply to: some text is selected, or the cursor is in a link.
	 * @param {Editor} [editor] - The editor to check.
	 * @returns {boolean} True when the link should be applied to existing text rather than inserted.
	 */
	#hasTextToLink(editor?: Editor): boolean {
		return !editor?.state.selection.empty || !!editor.isActive(UmbLink.name);
	}
}
