import type { UmbValidationMessageTranslator } from '../translators/validation-message-path-translator.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export type UmbValidationMessageType = 'client' | 'server';
export interface UmbValidationMessage {
	type: UmbValidationMessageType;
	key: string;
	path: string;
	body: string;
}

export class UmbValidationMessagesManager {
	#messages = new UmbArrayState<UmbValidationMessage>([], (x) => x.key);
	messages = this.#messages.asObservable();

	debug(logName: string) {
		this.#messages.asObservable().subscribe((x) => console.log(logName, x, this.#translators));
	}

	getHasAnyMessages(): boolean {
		return this.#messages.getValue().length !== 0;
	}

	messagesOfPathAndDescendant(path: string): Observable<Array<UmbValidationMessage>> {
		//path = path.toLowerCase();
		// Find messages that starts with the given path, if the path is longer then require a dot or [ as the next character. using a more performant way than Regex:
		return this.#messages.asObservablePart((msgs) =>
			msgs.filter(
				(x) =>
					x.path.indexOf(path) === 0 &&
					(x.path.length === path.length || x.path[path.length] === '.' || x.path[path.length] === '['),
			),
		);
	}

	messagesOfTypeAndPath(type: UmbValidationMessageType, path: string): Observable<Array<UmbValidationMessage>> {
		//path = path.toLowerCase();
		// Find messages that matches the given type and path.
		return this.#messages.asObservablePart((msgs) => msgs.filter((x) => x.type === type && x.path === path));
	}

	hasMessagesOfPathAndDescendant(path: string): Observable<boolean> {
		//path = path.toLowerCase();
		return this.#messages.asObservablePart((msgs) =>
			// Find messages that starts with the given path, if the path is longer then require a dot or [ as the next character. Using a more performant way than Regex: [NL]
			msgs.some(
				(x) =>
					x.path.indexOf(path) === 0 &&
					(x.path.length === path.length || x.path[path.length] === '.' || x.path[path.length] === '['),
			),
		);
	}
	getHasMessagesOfPathAndDescendant(path: string): boolean {
		//path = path.toLowerCase();
		return this.#messages
			.getValue()
			.some(
				(x) =>
					x.path.indexOf(path) === 0 &&
					(x.path.length === path.length || x.path[path.length] === '.' || x.path[path.length] === '['),
			);
	}

	addMessage(type: UmbValidationMessageType, path: string, body: string, key: string = UmbId.new()): void {
		//path = this.#translatePath(path.toLowerCase()) ?? path.toLowerCase();
		path = this.#translatePath(path) ?? path;
		// check if there is an existing message with the same path and type, and append the new messages: [NL]
		if (this.#messages.getValue().find((x) => x.type === type && x.path === path && x.body === body)) {
			return;
		}
		this.#messages.appendOne({ type, key, path, body: body });
	}

	addMessages(type: UmbValidationMessageType, path: string, bodies: Array<string>): void {
		//path = this.#translatePath(path.toLowerCase()) ?? path.toLowerCase();
		path = this.#translatePath(path) ?? path;
		// filter out existing messages with the same path and type, and append the new messages: [NL]
		const existingMessages = this.#messages.getValue();
		const newBodies = bodies.filter(
			(message) => existingMessages.find((x) => x.type === type && x.path === path && x.body === message) === undefined,
		);
		this.#messages.append(newBodies.map((body) => ({ type, key: UmbId.new(), path, body })));
	}

	removeMessageByKey(key: string): void {
		this.#messages.removeOne(key);
	}
	removeMessagesByTypeAndPath(type: UmbValidationMessageType, path: string): void {
		//path = path.toLowerCase();
		this.#messages.filter((x) => !(x.type === type && x.path === path));
	}
	removeMessagesByType(type: UmbValidationMessageType): void {
		this.#messages.filter((x) => x.type !== type);
	}

	#translatePath(path: string): string | undefined {
		//path = path.toLowerCase();
		for (const translator of this.#translators) {
			const newPath = translator.translate(path);
			// If not undefined or false, then it was a valid translation: [NL]
			if (newPath) {
				// Lets try to translate it again, this will recursively translate the path until no more translations are possible (and then fallback to '?? newpath') [NL]
				return this.#translatePath(newPath) ?? newPath;
			}
		}
		return;
	}

	#translators: Array<UmbValidationMessageTranslator> = [];
	addTranslator(translator: UmbValidationMessageTranslator): void {
		if (this.#translators.indexOf(translator) === -1) {
			this.#translators.push(translator);
		}
		// execute translators on all messages:
		// Notice we are calling getValue() in each iteration to avoid the need to re-translate the same messages over and over again. [NL]
		for (const msg of this.#messages.getValue()) {
			const newPath = this.#translatePath(msg.path);
			// If newPath is not false or undefined, a translation of it has occurred, meaning we ant to update it: [NL]
			if (newPath) {
				// update the specific message, with its new path: [NL]
				this.#messages.updateOne(msg.key, { path: newPath });
			}
		}
	}

	removeTranslator(translator: UmbValidationMessageTranslator): void {
		const index = this.#translators.indexOf(translator);
		if (index !== -1) {
			this.#translators.splice(index, 1);
		}
	}

	clear(): void {
		this.#messages.setValue([]);
	}

	destroy(): void {
		this.#translators = [];
		this.#messages.destroy();
	}
}
