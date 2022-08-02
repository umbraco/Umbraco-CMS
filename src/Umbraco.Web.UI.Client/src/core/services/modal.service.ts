import { BehaviorSubject, Observable } from 'rxjs';
import UmbModalHandler from './modalHandler';

export class UmbModalService {
	private _modals: BehaviorSubject<Array<UmbModalHandler>> = new BehaviorSubject(<Array<UmbModalHandler>>[]);
	public readonly modals: Observable<Array<UmbModalHandler>> = this._modals.asObservable();

	// public createModal(modalHandler: UmbModalHandler): void {
	// 	this._modals.next([...this._modals.getValue(), modalHandler]);
	// }

	public openSidebar(elementName: string): UmbModalHandler {
		return this._open(elementName, 'uui-modal-sidebar');
	}

	public openDialog(elementName: string): UmbModalHandler {
		return this._open(elementName, 'uui-modal-dialog');
	}

	private _open(elementName: string, modalElementName: string): UmbModalHandler {
		const modalHandler = new UmbModalHandler(elementName, modalElementName);
		modalHandler.onClose().then(() => this._close(modalHandler));
		this._modals.next([...this._modals.getValue(), modalHandler]);
		return modalHandler;
	}

	private _close(modalHandler: UmbModalHandler) {
		console.log('close', modalHandler);
		//TODO clicking escape doesn't trigger the cleanup
		modalHandler.modal.close();
		modalHandler.modal.addEventListener('close-end', () =>
			this._modals.next(this._modals.getValue().filter((modal) => modal.key !== modalHandler.key))
		);
	}
}
