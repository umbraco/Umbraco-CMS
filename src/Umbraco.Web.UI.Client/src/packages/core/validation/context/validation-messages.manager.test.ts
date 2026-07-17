import { expect } from '@open-wc/testing';
import { UmbValidationMessagesManager } from './validation-messages.manager';
import type { UmbValidationMessage } from './validation-messages.manager';
import type { UmbValidationMessageTranslator } from '../translators/validation-message-path-translator.interface.js';
import { UmbObserver } from '@umbraco-cms/backoffice/observable-api';

describe('UmbValidationMessagesManager', () => {
	let messages: UmbValidationMessagesManager;

	beforeEach(() => {
		messages = new UmbValidationMessagesManager();
	});

	it('knows if it has any messages', () => {
		messages.addMessage('server', '$.test', 'test');

		expect(messages.getHasAnyMessages()).to.be.true;
	});

	it('knows if it has any messages of a certain path or descending path', () => {
		messages.addMessage('server', `$.values[?(@.id == '123')].value`, 'test');

		expect(messages.getHasMessagesOfPathAndDescendant(`$.values[?(@.id == '123')]`)).to.be.true;
	});

	it('enables you to observe for path or descending path messages', async () => {
		messages.addMessage('server', `$.values[?(@.id == '123')].value`, 'test');

		const observeable = messages.hasMessagesOfPathAndDescendant(`$.values[?(@.id == '123')]`);

		const observer = new UmbObserver(observeable);
		const result = await observer.asPromise();

		expect(result).to.be.true;
	});

	describe('addMessage', () => {
		it('appends a message with an auto-generated key when no key is supplied', () => {
			messages.addMessage('server', '$.foo', 'body');
			const stored = messages.getMessages();
			expect(stored).to.have.length(1);
			expect(stored[0].key).to.be.a('string').and.have.length.greaterThan(0);
			expect(stored[0]).to.include({ type: 'server', path: '$.foo', body: 'body' });
		});

		it('uses the provided key when one is supplied', () => {
			messages.addMessage('server', '$.foo', 'body', 'my-explicit-key');
			expect(messages.getMessages()[0].key).to.equal('my-explicit-key');
		});

		it('deduplicates on (type, path, body) — adding the same triple twice keeps one entry', () => {
			messages.addMessage('server', '$.foo', 'body');
			messages.addMessage('server', '$.foo', 'body');
			expect(messages.getMessages()).to.have.length(1);
		});

		it('treats different types at the same path/body as distinct', () => {
			messages.addMessage('server', '$.foo', 'body');
			messages.addMessage('client', '$.foo', 'body');
			expect(messages.getMessages()).to.have.length(2);
		});

		it('treats different paths at the same type/body as distinct', () => {
			messages.addMessage('server', '$.foo', 'body');
			messages.addMessage('server', '$.bar', 'body');
			expect(messages.getMessages()).to.have.length(2);
		});

		it('treats different bodies at the same type/path as distinct', () => {
			messages.addMessage('server', '$.foo', 'first');
			messages.addMessage('server', '$.foo', 'second');
			expect(messages.getMessages()).to.have.length(2);
		});
	});

	describe('addMessages', () => {
		it('appends every body in the supplied list', () => {
			messages.addMessages('server', '$.foo', ['a', 'b', 'c']);
			expect(messages.getMessages().map((m) => m.body)).to.deep.equal(['a', 'b', 'c']);
		});

		it('skips bodies that already exist for the same type+path', () => {
			messages.addMessage('server', '$.foo', 'a');
			messages.addMessages('server', '$.foo', ['a', 'b']);
			expect(messages.getMessages().map((m) => m.body)).to.deep.equal(['a', 'b']);
		});

		it('does not skip when the same body exists at a different path', () => {
			messages.addMessage('server', '$.other', 'a');
			messages.addMessages('server', '$.foo', ['a']);
			expect(messages.getMessages()).to.have.length(2);
		});
	});

	describe('addMessageObjects', () => {
		// addMessageObjects is for context communication — it purposefully bypasses the (type, path, body) deduplication.
		it('appends every supplied object without deduplication', () => {
			const objs: Array<UmbValidationMessage> = [
				{ type: 'server', key: 'k1', path: '$.foo', body: 'x' },
				{ type: 'server', key: 'k2', path: '$.foo', body: 'x' },
			];
			messages.addMessageObjects(objs);
			expect(messages.getMessages()).to.have.length(2);
		});
	});

	describe('removeMessageByKey', () => {
		it('removes the single message by key', () => {
			messages.addMessage('server', '$.a', 'a', 'k1');
			messages.addMessage('server', '$.b', 'b', 'k2');
			messages.removeMessageByKey('k1');
			const remaining = messages.getMessages();
			expect(remaining).to.have.length(1);
			expect(remaining[0].key).to.equal('k2');
		});

		it('does nothing when the key is not present', () => {
			messages.addMessage('server', '$.a', 'a', 'k1');
			messages.removeMessageByKey('does-not-exist');
			expect(messages.getMessages()).to.have.length(1);
		});
	});

	describe('removeMessageByKeys', () => {
		it('removes every message whose key is in the supplied list', () => {
			messages.addMessage('server', '$.a', 'a', 'k1');
			messages.addMessage('server', '$.b', 'b', 'k2');
			messages.addMessage('server', '$.c', 'c', 'k3');
			messages.removeMessageByKeys(['k1', 'k3']);
			expect(messages.getMessages().map((m) => m.key)).to.deep.equal(['k2']);
		});

		it('returns early when the supplied list is empty', () => {
			messages.addMessage('server', '$.a', 'a', 'k1');
			messages.removeMessageByKeys([]);
			expect(messages.getMessages()).to.have.length(1);
		});
	});

	describe('removeMessagesByType', () => {
		// Pattern from server-model-validator: full replace of all 'server' messages.
		it('removes only messages of the given type', () => {
			messages.addMessage('server', '$.a', 'a');
			messages.addMessage('server', '$.b', 'b');
			messages.addMessage('client', '$.c', 'c');
			messages.removeMessagesByType('server');
			const remaining = messages.getMessages();
			expect(remaining).to.have.length(1);
			expect(remaining[0].type).to.equal('client');
		});
	});

	describe('removeMessagesByPath', () => {
		it('removes only messages with an exact path match', () => {
			messages.addMessage('server', '$.foo', 'a');
			messages.addMessage('server', '$.foo', 'b');
			messages.addMessage('server', '$.bar', 'c');
			messages.removeMessagesByPath('$.foo');
			const remaining = messages.getMessages();
			expect(remaining).to.have.length(1);
			expect(remaining[0].path).to.equal('$.bar');
		});

		it('does not remove descendant paths', () => {
			messages.addMessage('server', '$.foo', 'a');
			messages.addMessage('server', '$.foo.bar', 'b');
			messages.removeMessagesByPath('$.foo');
			const remaining = messages.getMessages();
			expect(remaining).to.have.length(1);
			expect(remaining[0].path).to.equal('$.foo.bar');
		});
	});

	describe('removeMessagesByTypeAndPath', () => {
		// Pattern from form-control-validator and value-validator —
		// scoped cleanup of one validator's contributions.
		it('removes only messages matching both type and path', () => {
			messages.addMessage('client', '$.foo', 'a');
			messages.addMessage('client', '$.foo', 'b');
			messages.addMessage('server', '$.foo', 'c');
			messages.addMessage('client', '$.bar', 'd');
			messages.removeMessagesByTypeAndPath('client', '$.foo');
			const remaining = messages.getMessages().map((m) => `${m.type}@${m.path}`);
			expect(remaining).to.deep.equal(['server@$.foo', 'client@$.bar']);
		});
	});

	describe('removeMessagesAndDescendantsByPath', () => {
		it('removes the path and its descendants and keeps other paths', () => {
			messages.addMessage('server', '$.foo', 'a');
			messages.addMessage('server', '$.foo.bar', 'b');
			messages.addMessage('server', '$.baz', 'c');

			messages.removeMessagesAndDescendantsByPath('$.foo');

			const paths = messages.getMessages().map((m) => m.path);
			expect(paths).to.not.contain('$.foo');
			expect(paths).to.not.contain('$.foo.bar');
			expect(paths).to.contain('$.baz');
		});

		it('does not remove paths that share the prefix but are not descendants', () => {
			messages.addMessage('server', '$.foo', 'a');
			messages.addMessage('server', '$.foobar', 'b');
			messages.removeMessagesAndDescendantsByPath('$.foo');
			const paths = messages.getMessages().map((m) => m.path);
			expect(paths).to.not.contain('$.foo');
			expect(paths).to.contain('$.foobar');
		});
	});

	describe('clear', () => {
		it('empties the message list', () => {
			messages.addMessage('server', '$.a', 'a');
			messages.addMessage('server', '$.b', 'b');
			messages.clear();
			expect(messages.getMessages()).to.have.length(0);
			expect(messages.getHasAnyMessages()).to.be.false;
		});
	});

	describe('getters', () => {
		it('getNotFilteredMessages returns the raw list, ignoring any active filter', () => {
			messages.addMessage('server', '$.a', 'a');
			messages.addMessage('client', '$.b', 'b');
			messages.filter((m) => m.type === 'server');
			expect(messages.getMessages()).to.have.length(1);
			expect(messages.getNotFilteredMessages()).to.have.length(2);
		});

		it('getMessagesOfPathAndDescendant matches the path itself and any descendant', () => {
			messages.addMessage('server', '$.foo', 'a');
			messages.addMessage('server', '$.foo.bar', 'b');
			messages.addMessage('server', '$.foo[0]', 'c');
			messages.addMessage('server', '$.foobar', 'd');
			messages.addMessage('server', '$.other', 'e');

			const matches = messages.getMessagesOfPathAndDescendant('$.foo').map((m) => m.path);

			expect(matches).to.have.members(['$.foo', '$.foo.bar', '$.foo[0]']);
			expect(matches).to.not.contain('$.foobar');
			expect(matches).to.not.contain('$.other');
		});

		it('getHasMessagesOfPathAndDescendant returns false when nothing matches', () => {
			messages.addMessage('server', '$.foo', 'a');
			expect(messages.getHasMessagesOfPathAndDescendant('$.bar')).to.be.false;
		});

		it('getHasMessageOfPathAndBody requires both fields to match exactly', () => {
			messages.addMessage('server', '$.foo', 'expected');
			expect(messages.getHasMessageOfPathAndBody('$.foo', 'expected')).to.be.true;
			expect(messages.getHasMessageOfPathAndBody('$.foo', 'other body')).to.be.false;
			expect(messages.getHasMessageOfPathAndBody('$.bar', 'expected')).to.be.false;
		});
	});

	describe('filter', () => {
		// Note: the filter does not retroactively re-emit; it only takes effect
		// on subsequent reads / emissions. (See the comment on filter() in the source.)
		it('applies to getMessages and filteredMessages observers', async () => {
			messages.addMessage('server', '$.a', 'a');
			messages.addMessage('client', '$.b', 'b');
			messages.filter((m) => m.type === 'server');

			expect(messages.getMessages().map((m) => m.type)).to.deep.equal(['server']);

			messages.addMessage('server', '$.c', 'c');
			const observed = await new UmbObserver(messages.filteredMessages).asPromise();
			expect(observed.every((m) => m.type === 'server')).to.be.true;
			expect(observed).to.have.length(2);
		});
	});

	describe('observables', () => {
		it('messagesOfTypeAndPath emits exact (type, path) matches only', async () => {
			messages.addMessage('server', '$.foo', 'a');
			messages.addMessage('client', '$.foo', 'b');
			messages.addMessage('server', '$.bar', 'c');
			messages.addMessage('server', '$.foo.child', 'd');

			const result = await new UmbObserver(messages.messagesOfTypeAndPath('server', '$.foo')).asPromise();
			expect(result).to.have.length(1);
			expect(result[0].body).to.equal('a');
		});

		it('messagesOfNotTypeAndPath emits same-path messages of any *other* type', async () => {
			// Pattern from bind-server-validation-to-form-control: surface
			// non-client (i.e. server / config) messages bound to a control.
			messages.addMessage('client', '$.foo', 'client-msg');
			messages.addMessage('server', '$.foo', 'server-msg');
			messages.addMessage('config', '$.foo', 'config-msg');
			messages.addMessage('server', '$.bar', 'other-path');

			const result = await new UmbObserver(messages.messagesOfNotTypeAndPath('client', '$.foo')).asPromise();
			expect(result.map((m) => m.body)).to.have.members(['server-msg', 'config-msg']);
		});

		it('messagesOfPathAndDescendant emits the matching path and any descendants', async () => {
			messages.addMessage('server', '$.foo', 'self');
			messages.addMessage('server', '$.foo.bar', 'child');
			messages.addMessage('server', '$.foobar', 'sibling');

			const result = await new UmbObserver(messages.messagesOfPathAndDescendant('$.foo')).asPromise();
			expect(result.map((m) => m.body)).to.have.members(['self', 'child']);
			expect(result.map((m) => m.body)).to.not.contain('sibling');
		});
	});

	describe('initiateChange/finishChange', () => {
		it('multiple messages added during a batch all surface after finishChange', () => {
			messages.initiateChange();
			messages.addMessage('server', '$.a', 'a');
			messages.addMessage('server', '$.b', 'b');
			messages.addMessage('server', '$.c', 'c');
			messages.finishChange();
			expect(messages.getMessages()).to.have.length(3);
		});

		it('uses a lock counter so nested initiateChange calls require matching finishChange calls', () => {
			messages.initiateChange();
			messages.initiateChange();
			messages.addMessage('server', '$.a', 'a');
			messages.finishChange();
			messages.addMessage('server', '$.b', 'b');
			messages.finishChange();
			expect(messages.getMessages()).to.have.length(2);
		});
	});

	describe('translators', () => {
		const translator = (from: string, to: string): UmbValidationMessageTranslator => ({
			translate: (path) => (path === from ? to : false),
		});

		it('rewrites the path on addMessage when a registered translator handles it', () => {
			messages.addTranslator(translator('$.aliasA', '$.aliasB'));
			messages.addMessage('server', '$.aliasA', 'body');
			expect(messages.getMessages()[0].path).to.equal('$.aliasB');
		});

		it('leaves the path untouched when no translator returns a value', () => {
			messages.addTranslator(translator('$.aliasA', '$.aliasB'));
			messages.addMessage('server', '$.untouched', 'body');
			expect(messages.getMessages()[0].path).to.equal('$.untouched');
		});

		it('treats a translator returning false as "not handled" and tries the next', () => {
			messages.addTranslator(translator('$.never', '$.never2'));
			messages.addTranslator(translator('$.aliasA', '$.aliasB'));
			messages.addMessage('server', '$.aliasA', 'body');
			expect(messages.getMessages()[0].path).to.equal('$.aliasB');
		});

		it('a translator returning undefined is also skipped', () => {
			const undefinedTranslator: UmbValidationMessageTranslator = { translate: () => undefined };
			messages.addTranslator(undefinedTranslator);
			messages.addMessage('server', '$.foo', 'body');
			expect(messages.getMessages()[0].path).to.equal('$.foo');
		});

		it('chains translators recursively (A → B, then B → C produces C)', () => {
			messages.addTranslator(translator('$.a', '$.b'));
			messages.addTranslator(translator('$.b', '$.c'));
			messages.addMessage('server', '$.a', 'body');
			expect(messages.getMessages()[0].path).to.equal('$.c');
		});

		it('addTranslator added after messages exist rewrites their paths in place', () => {
			messages.addMessage('server', '$.aliasA', 'body', 'fixed-key');
			messages.addTranslator(translator('$.aliasA', '$.aliasB'));
			const msg = messages.getMessages()[0];
			expect(msg.key).to.equal('fixed-key');
			expect(msg.path).to.equal('$.aliasB');
		});

		it('removeTranslator stops further translation but does not undo prior rewrites', () => {
			const t = translator('$.aliasA', '$.aliasB');
			messages.addTranslator(t);
			messages.addMessage('server', '$.aliasA', 'body');
			messages.removeTranslator(t);
			messages.addMessage('server', '$.aliasA', 'second');
			const paths = messages.getMessages().map((m) => m.path);
			expect(paths).to.have.members(['$.aliasB', '$.aliasA']);
		});

		it('addTranslator is idempotent — registering the same instance twice has no extra effect', () => {
			const t = translator('$.aliasA', '$.aliasB');
			messages.addTranslator(t);
			messages.addTranslator(t);
			messages.addMessage('server', '$.aliasA', 'body');
			expect(messages.getMessages()[0].path).to.equal('$.aliasB');
		});
	});

	describe('destroy', () => {
		it('throws when finishChange is called after destroy', () => {
			messages.initiateChange();
			messages.destroy();

			expect(() => messages.finishChange()).to.throw();
		});
	});
});
