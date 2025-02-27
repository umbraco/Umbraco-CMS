import type { UmbValidationMessageTranslator } from '../translators/validation-message-path-translator.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbArrayState, createObservablePart } from '@umbraco-cms/backoffice/observable-api';

export type UmbValidationMessageType = 'client' | 'server';
export interface UmbValidationMessage {
	type: UmbValidationMessageType;
	key: string;
	path: string;
	body: string;
}

/**
 * Matches a path or a descendant path.
 * @param {string} source The path to check.
 * @param {string} match The path to match against, the source must forfill all of the match, but the source can be further specific.
 * @returns {boolean} True if the path matches or is a descendant path.
 */
function MatchPathOrDescendantPath(source: string, match: string): boolean {
	// Find messages that starts with the given path, if the path is longer then require a dot or [ as the next character. using a more performant way than Regex:
	return (
		source.indexOf(match) === 0 &&
		(source.length === match.length || source[match.length] === '.' || source[match.length] === '[')
	);
}

export class UmbValidationMessagesManager {
	#filter?: (msg: UmbValidationMessage) => boolean;

	#messages = new UmbArrayState<UmbValidationMessage>([], (x) => x.key);
	messages = this.#messages.asObservable();
	filteredMessages = this.#messages.asObservablePart((msgs) => (this.#filter ? msgs.filter(this.#filter) : msgs));

	getFilteredMessages(): Array<UmbValidationMessage> {
		const msgs = this.#messages.getValue();
		return this.#filter ? msgs.filter(this.#filter) : msgs;
	}

	debug(logName: string) {
		this.messages.subscribe((x) => console.log(logName, x));
	}

	debugFiltered(logName: string) {
		this.filteredMessages.subscribe((x) => console.log(logName, x));
	}

	filter(method: (msg: UmbValidationMessage) => boolean): void {
		this.#filter = method;
		// This should maybe trigger a re-filter of the messages, but I'm not sure how we should do that properly of now. [NL]
		// The reason is that maybe the filter changes while we have validation messages.
	}

	#updateLock = 0;
	initiateChange() {
		this.#updateLock++;
		this.#messages.mute();
		// TODO: When ready enable this code will enable handling a finish automatically by this implementation 'using myState.initiatePropertyValueChange()' (Relies on TS support of Using) [NL]
		/*return {
			[Symbol.dispose]: this.finishPropertyValueChange,
		};*/
	}
	finishChange() {
		this.#updateLock--;
		if (this.#updateLock === 0) {
			this.#messages.unmute();
		}
	}

	getHasAnyMessages(): boolean {
		return this.getFilteredMessages().length !== 0;
	}

	getMessagesOfPathAndDescendant(path: string): Array<UmbValidationMessage> {
		//path = path.toLowerCase();
		return this.getFilteredMessages().filter((x) => MatchPathOrDescendantPath(x.path, path));
	}

	messagesOfPathAndDescendant(path: string): Observable<Array<UmbValidationMessage>> {
		//path = path.toLowerCase();
		return createObservablePart(this.filteredMessages, (msgs) =>
			msgs.filter((x) => MatchPathOrDescendantPath(x.path, path)),
		);
	}

	messagesOfTypeAndPath(type: UmbValidationMessageType, path: string): Observable<Array<UmbValidationMessage>> {
		//path = path.toLowerCase();
		// Find messages that matches the given type and path.
		return createObservablePart(this.filteredMessages, (msgs) =>
			msgs.filter((x) => x.type === type && x.path === path),
		);
	}

	hasMessagesOfPathAndDescendant(path: string): Observable<boolean> {
		//path = path.toLowerCase();
		return createObservablePart(this.filteredMessages, (msgs) =>
			msgs.some((x) => MatchPathOrDescendantPath(x.path, path)),
		);
	}
	getHasMessagesOfPathAndDescendant(path: string): boolean {
		//path = path.toLowerCase();
		return this.getFilteredMessages().some(
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
		this.initiateChange();
		this.#messages.appendOne({ type, key, path, body: body });
		this.finishChange();
	}

	addMessages(type: UmbValidationMessageType, path: string, bodies: Array<string>): void {
		//path = this.#translatePath(path.toLowerCase()) ?? path.toLowerCase();
		path = this.#translatePath(path) ?? path;
		// filter out existing messages with the same path and type, and append the new messages: [NL]
		const existingMessages = this.#messages.getValue();
		const newBodies = bodies.filter(
			(message) => existingMessages.find((x) => x.type === type && x.path === path && x.body === message) === undefined,
		);
		this.initiateChange();
		this.#messages.append(newBodies.map((body) => ({ type, key: UmbId.new(), path, body })));
		this.finishChange();
	}

	addMessageObjects(messages: Array<UmbValidationMessage>): void {
		this.initiateChange();
		this.#messages.append(messages);
		this.finishChange();
	}

	removeMessageByKey(key: string): void {
		this.initiateChange();
		this.#messages.removeOne(key);
		this.finishChange();
	}
	removeMessageByKeys(keys: Array<string>): void {
		this.initiateChange();
		this.#messages.filter((x) => keys.indexOf(x.key) === -1);
		this.finishChange();
	}
	removeMessagesByType(type: UmbValidationMessageType): void {
		this.initiateChange();
		this.#messages.filter((x) => x.type !== type);
		this.finishChange();
	}
	removeMessagesByPath(path: string): void {
		this.initiateChange();
		this.#messages.filter((x) => x.path !== path);
		this.finishChange();
	}
	removeMessagesAndDescendantsByPath(path: string): void {
		this.initiateChange();
		this.#messages.filter((x) => MatchPathOrDescendantPath(x.path, path));
		this.finishChange();
	}
	removeMessagesByTypeAndPath(type: UmbValidationMessageType, path: string): void {
		//path = path.toLowerCase();
		this.initiateChange();
		this.#messages.filter((x) => !(x.type === type && x.path === path));
		this.finishChange();
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
		this.initiateChange();
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
		this.finishChange();
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
