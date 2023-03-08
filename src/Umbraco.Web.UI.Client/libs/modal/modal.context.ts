// TODO: lazy load
import './layouts/confirm/modal-layout-confirm.element';
import '../../src/backoffice/documents/documents/modals/document-picker/document-picker-modal.element';
import './layouts/media-picker/modal-layout-media-picker.element';
import './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';
import '../../src/backoffice/users/current-user/modals/current-user/current-user-modal.element';
import '../../src/backoffice/shared/modals/icon-picker/icon-picker-modal.element';
import '../../src/backoffice/settings/languages/language-picker/language-picker-modal-layout.element';
import './layouts/link-picker/modal-layout-link-picker.element';
import './layouts/basic/modal-layout-basic.element';
import './layouts/search/modal-layout-search.element.ts';

import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { BehaviorSubject } from 'rxjs';
import type { UUIModalDialogElement } from '@umbraco-ui/uui-modal-dialog';
import type { UmbModalDocumentPickerData } from '../../src/backoffice/documents/documents/modals/document-picker';
import { UmbModalChangePasswordData } from './layouts/modal-layout-change-password.element';
import { UmbIconPickerModalData } from '../../src/backoffice/shared/modals/icon-picker';
import type { UmbModalConfirmData } from './layouts/confirm/modal-layout-confirm.element';
import type { UmbModalPropertyEditorUIPickerData } from './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';
import type { UmbModalMediaPickerData } from './layouts/media-picker/modal-layout-media-picker.element';
import type { UmbModalLinkPickerData } from './layouts/link-picker/modal-layout-link-picker.element';
import { UmbModalHandler } from './modal-handler';
import type { UmbBasicModalData } from './layouts/basic/modal-layout-basic.element';
import { UmbPickerModalData } from './layouts/modal-layout-picker-base';
import { UmbModalToken } from './token/modal-token';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { LanguageModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export type UmbModalType = 'dialog' | 'sidebar';

export interface UmbModalConfig {
	type?: UmbModalType;
	size?: UUIModalSidebarSize;
}

// TODO: we should find a way to easily open a modal without adding custom methods to this context. It would result in a better separation of concerns.
// TODO: move all layouts into their correct "silo" folders. User picker should live with users etc.
export class UmbModalContext {
	host: UmbControllerHostInterface;
	// TODO: Investigate if we can get rid of HTML elements in our store, so we can use one of our states.
	#modals = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals = this.#modals.asObservable();

	constructor(host: UmbControllerHostInterface) {
		this.host = host;
	}

	/**
	 * Opens a Confirm modal
	 * @public
	 * @param {UmbModalConfirmData} data
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public confirm(data: UmbModalConfirmData): UmbModalHandler {
		return this.open('umb-modal-layout-confirm', { data, type: 'dialog' });
	}

	/**
	 * Opens a Content Picker sidebar modal
	 * @public
	 * @param {UmbModalDocumentPickerData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public documentPicker(data?: UmbModalDocumentPickerData): UmbModalHandler {
		return this.open('umb-document-picker-modal', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens a Media Picker sidebar modal
	 * @public
	 * @param {UmbModalMediaPickerData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public mediaPicker(data?: UmbModalMediaPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-media-picker', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens a Property Editor UI sidebar modal
	 * @public
	 * @param {UmbModalPropertyEditorUIPickerData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public propertyEditorUIPicker(data?: UmbModalPropertyEditorUIPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-property-editor-ui-picker', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens an Icon Picker sidebar modal
	 * @public
	 * @param {UmbIconPickerModalData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public iconPicker(data?: UmbIconPickerModalData): UmbModalHandler {
		return this.open('umb-modal-layout-icon-picker', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens an Link Picker sidebar modal
	 * @public
	 * @param {(LinkPickerData & LinkPickerConfig)} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public linkPicker(data?: UmbModalLinkPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-link-picker', {
			data,
			type: 'sidebar',
			size: data?.config?.overlaySize || 'small',
		});
	}

	/**
	 * Opens the user settings sidebar modal
	 * @public
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public userSettings(): UmbModalHandler {
		return this.open('umb-modal-layout-current-user', { type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens the change password sidebar modal
	 * @public
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public changePassword(data: UmbModalChangePasswordData): UmbModalHandler {
		return this.open('umb-modal-layout-change-password', { data, type: 'dialog' });
	}

	/**
	 * Opens a language picker sidebar modal
	 * @public
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public languagePicker(data: UmbPickerModalData<LanguageModel>): UmbModalHandler {
		return this.open('umb-language-picker-modal-layout', { data, type: 'sidebar' });
	}

	/**
	 * Opens a basic sidebar modal to display readonly information
	 * @public
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public openBasic(data: UmbBasicModalData): UmbModalHandler {
		return this.open('umb-modal-layout-basic', {
			data,
			type: 'sidebar',
			size: data?.overlaySize || 'small',
		});
	}

	public search(): UmbModalHandler {
		const modalHandler = new UmbModalHandler('umb-modal-layout-search');

		//TODO START: This is a hack to get the search modal layout to look like i want it to.
		//TODO: Remove from here to END when the modal system is more flexible
		const topDistance = '50%';
		const margin = '16px';
		const maxHeight = '600px';
		const maxWidth = '500px';
		const dialog = document.createElement('dialog') as HTMLDialogElement;
		dialog.style.top = `max(${margin}, calc(${topDistance} - ${maxHeight} / 2))`;
		dialog.style.margin = '0 auto';
		dialog.style.transform = `translateY(${-maxHeight})`;
		dialog.style.maxHeight = `min(${maxHeight}, calc(100% - ${margin}px * 2))`;
		dialog.style.width = `min(${maxWidth}, calc(100vw - ${margin}))`;
		dialog.style.boxSizing = 'border-box';
		dialog.style.background = 'none';
		dialog.style.border = 'none';
		dialog.style.padding = '0';
		dialog.style.boxShadow = 'var(--uui-shadow-depth-5)';
		dialog.style.borderRadius = '9px';
		const search = document.createElement('umb-modal-layout-search');
		dialog.appendChild(search);
		requestAnimationFrame(() => {
			dialog.showModal();
		});
		modalHandler.element = dialog as unknown as UUIModalDialogElement;
		//TODO END

		modalHandler.element.addEventListener('close-end', () => this.#onCloseEnd(modalHandler));

		this.#modals.next([...this.#modals.getValue(), modalHandler]);
		return modalHandler;
	}

	/**
	 * Opens a modal or sidebar modal
	 * @public
	 * @param {(string | HTMLElement)} element
	 * @param {UmbModalOptions<unknown>} [options]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalContext
	 */
	public open<T = unknown>(modalAlias: string | UmbModalToken<T>, data?: T, config?: UmbModalConfig): UmbModalHandler {
		const modalHandler = new UmbModalHandler(this.host, modalAlias, data, config);

		modalHandler.containerElement.addEventListener('close-end', () => this.#onCloseEnd(modalHandler));

		this.#modals.next([...this.#modals.getValue(), modalHandler]);
		return modalHandler;
	}

	/**
	 * Closes a modal or sidebar modal
	 * @private
	 * @param {string} key
	 * @memberof UmbModalContext
	 */
	private _close(key: string) {
		this.#modals.next(this.#modals.getValue().filter((modal) => modal.key !== key));
	}

	/**
	 * Handles the close-end event
	 * @private
	 * @param {UmbModalHandler} modalHandler
	 * @memberof UmbModalContext
	 */
	#onCloseEnd(modalHandler: UmbModalHandler) {
		modalHandler.containerElement.removeEventListener('close-end', () => this.#onCloseEnd(modalHandler));
		this._close(modalHandler.key);
	}
}

export const UMB_MODAL_CONTEXT_TOKEN = new UmbContextToken<UmbModalContext>('UmbModalContext');
