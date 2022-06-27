import { BehaviorSubject, Observable } from 'rxjs';

type TempNotificationObject = {
  headline: string;
  key: string;
};

export class UmbNotificationService {
  private _notifications: BehaviorSubject<Array<TempNotificationObject>> = new BehaviorSubject(
    <Array<TempNotificationObject>>[]
  );
  public readonly notifications: Observable<Array<TempNotificationObject>> = this._notifications.asObservable();

  // TODO: this is just a quick solution to get notifications in POC. (suppose to get much more complex data set for this, enabling description, actions and event custom elements).
  peek(headline: string) {
    this._notifications.next([...this._notifications.getValue(), { headline: headline, key: Date.now().toString() }]);
  }
}
