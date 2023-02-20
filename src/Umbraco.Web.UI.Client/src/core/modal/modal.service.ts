// TODO: lazy load
import './layouts/confirm/modal-layout-confirm.element';
import './layouts/content-picker/modal-layout-content-picker.element';
import './layouts/media-picker/modal-layout-media-picker.element';
import './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';
import './layouts/modal-layout-current-user.element';
import './layouts/icon-picker/modal-layout-icon-picker.element';
import '../../backoffice/settings/languages/language-picker/language-picker-modal-layout.element';

import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { BehaviorSubject } from 'rxjs';
import { UmbModalChangePasswordData } from './layouts/modal-layout-change-password.element';
import type { UmbModalIconPickerData } from './layouts/icon-picker/modal-layout-icon-picker.element';
import type { UmbModalConfirmData } from './layouts/confirm/modal-layout-confirm.element';
import type { UmbModalContentPickerData } from './layouts/content-picker/modal-layout-content-picker.element';
import type { UmbModalPropertyEditorUIPickerData } from './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';
import type { UmbModalMediaPickerData } from './layouts/media-picker/modal-layout-media-picker.element';
import { UmbModalHandler } from './modal-handler';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbLanguagePickerModalData } from '../../backoffice/settings/languages/language-picker/language-picker-modal-layout.element';

export type UmbModalType = 'dialog' | 'sidebar';

export interface UmbModalOptions<UmbModalData> {
	type?: UmbModalType;
	size?: UUIModalSidebarSize;
	data?: UmbModalData;
}

// TODO: rename to UmbModalContext
// TODO: we should find a way to easily open a modal without adding custom methods to this context. It would result in a better separation of concerns.
// TODO: move all layouts into their correct "silo" folders. User picker should live with users etc.
export class UmbModalService {
	// TODO: Investigate if we can get rid of HTML elements in our store, so we can use one of our states.
	#modals = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals = this.#modals.asObservable();

	/**
	 * Opens a Confirm modal
	 * @public
	 * @param {UmbModalConfirmData} data
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public confirm(data: UmbModalConfirmData): UmbModalHandler {
		return this.open('umb-modal-layout-confirm', { data, type: 'dialog' });
	}

	/**
	 * Opens a Content Picker sidebar modal
	 * @public
	 * @param {UmbModalContentPickerData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public contentPicker(data?: UmbModalContentPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-content-picker', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens a Media Picker sidebar modal
	 * @public
	 * @param {UmbModalMediaPickerData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public mediaPicker(data?: UmbModalMediaPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-media-picker', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens a Property Editor UI sidebar modal
	 * @public
	 * @param {UmbModalPropertyEditorUIPickerData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public propertyEditorUIPicker(data?: UmbModalPropertyEditorUIPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-property-editor-ui-picker', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens an Icon Picker sidebar modal
	 * @public
	 * @param {UmbModalIconPickerData} [data]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public iconPicker(data?: UmbModalIconPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-icon-picker', { data, type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens the user settings sidebar modal
	 * @public
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public userSettings(): UmbModalHandler {
		return this.open('umb-modal-layout-current-user', { type: 'sidebar', size: 'small' });
	}

	/**
	 * Opens the user settings sidebar modal
	 * @public
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public changePassword(data: UmbModalChangePasswordData): UmbModalHandler {
		return this.open('umb-modal-layout-change-password', { data, type: 'dialog' });
	}

	/**
	 * Opens a language picker sidebar modal
	 * @public
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public languagePicker(data: UmbLanguagePickerModalData): UmbModalHandler {
		return this.open('umb-language-picker-modal-layout', { data, type: 'sidebar' });
	}

	/**
	 * Opens a modal or sidebar modal
	 * @public
	 * @param {(string | HTMLElement)} element
	 * @param {UmbModalOptions<unknown>} [options]
	 * @return {*}  {UmbModalHandler}
	 * @memberof UmbModalService
	 */
	public open(element: string | HTMLElement, options?: UmbModalOptions<unknown>): UmbModalHandler {
		const modalHandler = new UmbModalHandler(element, options);

		modalHandler.element.addEventListener('close-end', () => this._handleCloseEnd(modalHandler));

		this.#modals.next([...this.#modals.getValue(), modalHandler]);
		return modalHandler;
	}

	/**
	 * Closes a modal or sidebar modal
	 * @private
	 * @param {string} key
	 * @memberof UmbModalService
	 */
	private _close(key: string) {
		this.#modals.next(this.#modals.getValue().filter((modal) => modal.key !== key));
	}

	/**
	 * Handles the close-end event
	 * @private
	 * @param {UmbModalHandler} modalHandler
	 * @memberof UmbModalService
	 */
	private _handleCloseEnd(modalHandler: UmbModalHandler) {
		modalHandler.element.removeEventListener('close-end', () => this._handleCloseEnd(modalHandler));
		this._close(modalHandler.key);
	}
}

export const UMB_MODAL_SERVICE_CONTEXT_TOKEN = new UmbContextToken<UmbModalService>(UmbModalService.name);
