import { BehaviorSubject, Observable } from 'rxjs';
import { UmbModalHandler } from './';
import type { UmbModalConfirmData } from './layouts/confirm/modal-layout-confirm.element';
import type { UmbModalContentPickerData } from './layouts/content-picker/modal-layout-content-picker.element';
import type { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import type { UmbModalPropertyEditorUIPickerData } from './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';

// TODO: remove these imports when they are part of UUI
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-sidebar';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';

// TODO: lazy load
import './layouts/confirm/modal-layout-confirm.element';
import './layouts/content-picker/modal-layout-content-picker.element';
import './layouts/property-editor-ui-picker/modal-layout-property-editor-ui-picker.element';

export type UmbModelType = 'dialog' | 'sidebar';

export interface UmbModalOptions<UmbModalData> {
	type?: UmbModelType;
	size?: UUIModalSidebarSize;
	data?: UmbModalData;
}

export class UmbModalService {
	private _modals: BehaviorSubject<Array<UmbModalHandler>> = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals: Observable<Array<UmbModalHandler>> = this._modals.asObservable();

	public confirm(data: UmbModalConfirmData): UmbModalHandler {
		return this.open('umb-modal-layout-confirm', { data, type: 'dialog' });
	}

	public contentPicker(data?: UmbModalContentPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-content-picker', { data, type: 'sidebar', size: 'small' });
	}

	public propertyEditorUIPicker(data?: UmbModalPropertyEditorUIPickerData): UmbModalHandler {
		return this.open('umb-modal-layout-property-editor-ui-picker', { data, type: 'sidebar', size: 'small' });
	}

	public open(elementName: string, options?: UmbModalOptions<unknown>): UmbModalHandler {
		const modalHandler = new UmbModalHandler(elementName, options);

		modalHandler.element.addEventListener('close-end', () => this._handleCloseEnd(modalHandler));

		this._modals.next([...this._modals.getValue(), modalHandler]);
		return modalHandler;
	}

	private _close(key: string) {
		this._modals.next(this._modals.getValue().filter((modal) => modal.key !== key));
	}

	private _handleCloseEnd(modalHandler: UmbModalHandler) {
		modalHandler.element.removeEventListener('close-end', () => this._handleCloseEnd(modalHandler));
		this._close(modalHandler.key);
	}
}
