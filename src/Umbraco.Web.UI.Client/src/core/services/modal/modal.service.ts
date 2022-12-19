// TODO: lazy load
import './layouts/confirm/modal-layout-confirm.element';
import './layouts/content-picker/modal-layout-content-picker.element';
import './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';
import './layouts/modal-layout-current-user.element';

import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { BehaviorSubject, Observable } from 'rxjs';
import { UmbModalChangePasswordData } from './layouts/modal-layout-change-password.element';
import { UmbModalIconPickerData } from './layouts/icon-picker/modal-layout-icon-picker.element';

import type { UmbModalConfirmData } from './layouts/confirm/modal-layout-confirm.element';
import type { UmbModalContentPickerData } from './layouts/content-picker/modal-layout-content-picker.element';
import type { UmbModalPropertyEditorUIPickerData } from './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';
import { UmbModalHandler } from './';

export type UmbModalType = 'dialog' | 'sidebar';

export interface UmbModalOptions<UmbModalData> {
	type?: UmbModalType;
	size?: UUIModalSidebarSize;
	data?: UmbModalData;
}

export class UmbModalService {
	private _modals: BehaviorSubject<Array<UmbModalHandler>> = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals: Observable<Array<UmbModalHandler>> = this._modals.asObservable();

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

		this._modals.next([...this._modals.getValue(), modalHandler]);
		return modalHandler;
	}

	/**
	 * Closes a modal or sidebar modal
	 * @private
	 * @param {string} key
	 * @memberof UmbModalService
	 */
	private _close(key: string) {
		this._modals.next(this._modals.getValue().filter((modal) => modal.key !== key));
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
