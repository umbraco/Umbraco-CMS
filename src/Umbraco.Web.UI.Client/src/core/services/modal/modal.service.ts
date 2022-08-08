import { BehaviorSubject, Observable } from 'rxjs';
import { UmbModalHandler } from './';

export class UmbModalService {
	private _modals: BehaviorSubject<Array<UmbModalHandler>> = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals: Observable<Array<UmbModalHandler>> = this._modals.asObservable();

	public openSidebar(elementName: string, modalOptions?: any): UmbModalHandler {
		return this._open(elementName, 'uui-modal-sidebar', modalOptions);
	}

	public openDialog(elementName: string, modalOptions?: any): UmbModalHandler {
		return this._open(elementName, 'uui-modal-dialog', modalOptions);
	}

	private _open(elementName: string, modalElementName: string, modalOptions?: any): UmbModalHandler {
		const modalHandler = new UmbModalHandler(elementName, modalElementName, modalOptions);
		modalHandler.onClose.then(() => this._close(modalHandler));
		this._modals.next([...this._modals.getValue(), modalHandler]);
		return modalHandler;
	}

	private _close(modalHandler: UmbModalHandler) {
		this._modals.next(this._modals.getValue().filter((modal) => modal.key !== modalHandler.key));
	}
}
