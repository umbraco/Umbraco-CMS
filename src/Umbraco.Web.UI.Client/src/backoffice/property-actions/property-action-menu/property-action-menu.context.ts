import { Observable, ReplaySubject } from 'rxjs';

export class UmbPropertyActionMenuContext {
  private _isOpen: ReplaySubject<boolean> = new ReplaySubject(1);
  public readonly isOpen: Observable<boolean> = this._isOpen.asObservable();

  open () {
    this._isOpen.next(true);
  }

  close () {
    this._isOpen.next(false);
  }
}