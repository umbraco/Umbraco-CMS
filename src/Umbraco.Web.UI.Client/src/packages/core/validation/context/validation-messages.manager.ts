import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export type UmbValidationMessageType = 'client' | 'server';
export interface UmbValidationMessage {
	type: UmbValidationMessageType;
	key: string;
	path: string;
	message: string;
}

export class UmbValidationMessagesManager {
	#messages = new UmbArrayState<UmbValidationMessage>([], (x) => x.key);

	/*constructor() {
		this.#messages.asObservable().subscribe((x) => console.log('messages:', x));
	}*/

	/*
	serializeMessages(fromPath: string, toPath: string): void {
		this.#messages.setValue(
			this.#messages.getValue().map((x) => {
				if (x.path.indexOf(fromPath) === 0) {
					x.path = toPath + x.path.substring(fromPath.length);
				}
				return x;
			}),
		);
	}
	*/

	getHasAnyMessages(): boolean {
		return this.#messages.getValue().length !== 0;
	}

	/*
	messagesOfPathAndDescendant(path: string): Observable<Array<UmbValidationMessage>> {
		// Find messages that starts with the given path, if the path is longer then require a dot or [ as the next character. using a more performant way than Regex:
		return this.#messages.asObservablePart((msgs) =>
			msgs.filter(
				(x) =>
					x.path.indexOf(path) === 0 &&
					(x.path.length === path.length || x.path[path.length] === '.' || x.path[path.length] === '['),
			),
		);
	}
	*/

	messagesOfTypeAndPath(type: UmbValidationMessageType, path: string): Observable<Array<UmbValidationMessage>> {
		// Find messages that matches the given type and path.
		return this.#messages.asObservablePart((msgs) => msgs.filter((x) => x.type === type && x.path === path));
	}

	hasMessagesOfPathAndDescendant(path: string): Observable<boolean> {
		return this.#messages.asObservablePart((msgs) =>
			// Find messages that starts with the given path, if the path is longer then require a dot or [ as the next character. Using a more performant way than Regex:
			msgs.some(
				(x) =>
					x.path.indexOf(path) === 0 &&
					(x.path.length === path.length || x.path[path.length] === '.' || x.path[path.length] === '['),
			),
		);
	}
	getHasMessagesOfPathAndDescendant(path: string): boolean {
		return this.#messages
			.getValue()
			.some(
				(x) =>
					x.path.indexOf(path) === 0 &&
					(x.path.length === path.length || x.path[path.length] === '.' || x.path[path.length] === '['),
			);
	}

	addMessage(type: UmbValidationMessageType, path: string, message: string): void {
		this.#messages.appendOne({ type, key: UmbId.new(), path, message });
	}

	addMessages(type: UmbValidationMessageType, path: string, messages: Array<string>): void {
		this.#messages.append(messages.map((message) => ({ type, key: UmbId.new(), path, message })));
	}

	/*
	removeMessage(message: UmbValidationDataPath): void {
		this.#messages.removeOne(message.key);
	}*/
	removeMessageByKey(key: string): void {
		this.#messages.removeOne(key);
	}
	removeMessagesByTypeAndPath(type: UmbValidationMessageType, path: string): void {
		this.#messages.filter((x) => !(x.type === type && x.path === path));
	}
	removeMessagesByType(type: UmbValidationMessageType): void {
		this.#messages.filter((x) => x.type !== type);
	}

	reset(): void {
		this.#messages.setValue([]);
	}

	destroy(): void {
		this.#messages.destroy();
	}
}
