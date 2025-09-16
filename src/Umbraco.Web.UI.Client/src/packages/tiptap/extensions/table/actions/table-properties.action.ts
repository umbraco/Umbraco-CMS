import { UmbTiptapMenuItemActionApi } from '../../../components/menu/tiptap-menu-item-api-base.js';
import { UMB_TIPTAP_RTE_CONTEXT } from '../../../contexts/tiptap-rte.context-token.js';
import { UMB_TIPTAP_TABLE_PROPERTIES_MODAL } from '../modals/table-properties-modal.token.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTablePropertiesAction extends UmbTiptapMenuItemActionApi {
	async #tableProperties(editor?: Editor) {
		if (!editor || !editor.isActive('table')) return;

		const modalData = this.#getModalData(editor);
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager?.open(this, UMB_TIPTAP_TABLE_PROPERTIES_MODAL, modalData);

		if (!modal) return;

		const data = await modal.onSubmit().catch(() => undefined);
		if (!data) return;

		const style = this.#getStyles(data);
		if (!style) return;

		editor?.chain().focus().updateAttributes('table', { style }).run();
	}

	#getModalData(editor?: Editor) {
		const tableStyles = (editor?.getAttributes('table').style as string) ?? '';
		const table = document.createElement('table');
		table.style.cssText = tableStyles;

		const data: Record<string, unknown> = {};

		data.alignment = this.#getAlignment(table.style);
		if (table.style.backgroundColor) data.backgroundColor = table.style.backgroundColor;
		if (table.style.borderColor) data.borderColor = table.style.borderColor;
		if (table.style.borderStyle) data.borderStyle = table.style.borderStyle;
		if (table.style.borderWidth) data.borderWidth = table.style.borderWidth;
		if (table.style.height) data.height = table.style.height;
		if (table.style.width) data.width = table.style.width;

		return { data };
	}

	#getAlignment(style: CSSStyleDeclaration) {
		if (style.marginLeft === 'auto' && style.marginRight === 'auto') {
			return 'center';
		} else if (style.marginRight === 'auto') {
			return 'left';
		} else if (style.marginLeft === 'auto') {
			return 'right';
		}
		return 'none';
	}

	#getStyles(data: typeof UMB_TIPTAP_TABLE_PROPERTIES_MODAL.VALUE) {
		const styles: Record<string, unknown> = {};

		// TODO: Move this to a shared utility function. [LK]
		const camelCaseToKebabCase = (str: string): string => {
			return str.replace(/[A-Z]+(?![a-z])|[A-Z]/g, ($, ofs) => (ofs ? '-' : '') + $.toLowerCase());
		};

		for (const item of data) {
			if (!item.value) continue;

			switch (item.alias) {
				case 'alignment': {
					const alignment =
						Array.isArray(item.value) && item.value.length ? (item.value[0] as string) : ((item.value as string) ?? '');
					switch (alignment) {
						case 'left':
							styles['margin-right'] = 'auto';
							break;
						case 'center':
							styles['margin-left'] = 'auto';
							styles['margin-right'] = 'auto';
							break;
						case 'right':
							styles['margin-left'] = 'auto';
							break;
						default:
							styles['margin-left'] = 'none';
							styles['margin-right'] = 'none';
							break;
					}
					break;
				}

				case 'borderStyle': {
					const borderStyle =
						Array.isArray(item.value) && item.value.length ? (item.value[0] as string) : ((item.value as string) ?? '');
					if (borderStyle) styles['border-style'] = borderStyle;
					break;
				}

				case 'backgroundColor':
				case 'borderColor':
				case 'borderWidth':
				case 'height':
				case 'width': {
					const propertyName = camelCaseToKebabCase(item.alias);
					styles[propertyName] = item.value;
					break;
				}

				default:
					break;
			}
		}

		return Object.entries(styles)
			.map(([key, value]) => `${key}: ${value}`)
			.join(';');
	}

	override async execute() {
		const ctx = await this.getContext(UMB_TIPTAP_RTE_CONTEXT);
		this.#tableProperties(ctx?.getEditor());
	}
}
