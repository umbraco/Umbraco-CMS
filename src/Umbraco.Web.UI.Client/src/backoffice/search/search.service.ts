import './search.element';
import type { UmbSearchElement } from './search.element';

export class UmbSearchService {
	public static async Open() {
		const topDistance = '128px';
		const margin = '16px';
		const maxHeight = '600px';
		const maxWidth = '500px';
		const dialog = document.createElement('dialog') as HTMLDialogElement;
		dialog.style.top = `min(${topDistance}, 10vh)`;
		dialog.style.margin = '0 auto';
		dialog.style.height = `min(${maxHeight}, calc(100vh - ${margin}))`;
		dialog.style.width = `min(${maxWidth}, calc(100vw - ${margin}))`;
		dialog.style.boxSizing = 'border-box';
		dialog.style.background = 'none';
		dialog.style.border = 'none';
		dialog.style.padding = '0';
		dialog.style.boxShadow = 'var(--uui-shadow-depth-5)';
		dialog.style.borderRadius = '9px';
		const search = document.createElement('umb-search') as UmbSearchElement;
		dialog.appendChild(search);

		//TODO: Yeah... This is not final
		const backoffice =
			document.body.children[0].shadowRoot?.children[0].shadowRoot?.children[0].children[0].shadowRoot?.querySelector(
				'umb-backoffice-modal-container'
			)?.shadowRoot?.children[0].shadowRoot;

		backoffice?.appendChild(dialog);
		dialog.showModal();
	}
}
