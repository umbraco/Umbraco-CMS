import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

interface UmbValidationMessage {
	key: string;
	path: string;
	message: string;
}

export class UmbValidationMessagesManager {
	#messages = new UmbArrayState<UmbValidationMessage>([], (x) => x.key);

	messagesOf(path: string): Observable<Array<UmbValidationMessage>> {
		return this.#messages.asObservablePart((x) => x.filter((y) => y.path.indexOf(path) === 0));
	}

	hasMessagesOf(path: string): Observable<boolean> {
		return this.#messages.asObservablePart((x) => x.some((y) => y.path.indexOf(path) === 0));
	}

	addMessage(path: string, message: string): void {
		this.#messages.appendOne({ key: UmbId.new(), path, message });
	}

	/*
	removeMessage(message: UmbValidationDataPath): void {
		this.#messages.removeOne(message.key);
	}*/
	removeMessageByKey(key: string): void {
		this.#messages.removeOne(key);
	}
	/*removeMessagesByPath(path: string): void {
		
	}*/

	reset(): void {
		this.#messages.setValue([]);
	}

	destroy(): void {
		this.#messages.destroy();
	}
}
