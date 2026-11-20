import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_USER_START_NODE_ACCESS_CONDITION_ALIAS } from './constants.js';
import { UmbMediaUserStartNodeAccessCondition } from './media-user-start-node-access.condition.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-media-user-start-node-access-host')
class UmbTestHostElement extends UmbElementMixin(HTMLElement) {
	#hasMediaRootAccess = new UmbBasicState<boolean | undefined>(undefined);
	#entityType = new UmbBasicState<string | undefined>(undefined);

	constructor() {
		super();
		this.provideContext(UMB_CURRENT_USER_CONTEXT, {
			getHostElement: () => this,
			hasMediaRootAccess: this.#hasMediaRootAccess.asObservable(),
		} as never);
		this.provideContext(UMB_ENTITY_CONTEXT, {
			getHostElement: () => this,
			entityType: this.#entityType.asObservable(),
		} as never);
	}

	setHasMediaRootAccess(value: boolean | undefined) {
		this.#hasMediaRootAccess.setValue(value);
	}

	setEntityType(value: string | undefined) {
		this.#entityType.setValue(value);
	}
}

describe('UmbMediaUserStartNodeAccessCondition', () => {
	let host: UmbTestHostElement;
	let condition: UmbMediaUserStartNodeAccessCondition;

	beforeEach(() => {
		host = new UmbTestHostElement();
		document.body.appendChild(host);
		condition = new UmbMediaUserStartNodeAccessCondition(host, {
			host,
			config: { alias: UMB_MEDIA_USER_START_NODE_ACCESS_CONDITION_ALIAS },
			onChange: () => {},
		});
	});

	afterEach(() => {
		condition.destroy();
		document.body.innerHTML = '';
	});

	it('permits the media root when the user has root access', async () => {
		host.setHasMediaRootAccess(true);
		host.setEntityType(UMB_MEDIA_ROOT_ENTITY_TYPE);
		await aTimeout(0);
		expect(condition.permitted).to.be.true;
	});

	it('refuses the media root when the user has no root access', async () => {
		host.setHasMediaRootAccess(false);
		host.setEntityType(UMB_MEDIA_ROOT_ENTITY_TYPE);
		await aTimeout(0);
		expect(condition.permitted).to.be.false;
	});

	it('refuses the media root while root access is not yet known', async () => {
		host.setEntityType(UMB_MEDIA_ROOT_ENTITY_TYPE);
		await aTimeout(0);
		expect(condition.permitted).to.be.false;
	});

	it('permits a media item without root access', async () => {
		host.setHasMediaRootAccess(false);
		host.setEntityType(UMB_MEDIA_ENTITY_TYPE);
		await aTimeout(0);
		expect(condition.permitted).to.be.true;
	});

	it('re-evaluates when root access changes', async () => {
		host.setEntityType(UMB_MEDIA_ROOT_ENTITY_TYPE);
		host.setHasMediaRootAccess(true);
		await aTimeout(0);
		expect(condition.permitted).to.be.true;

		host.setHasMediaRootAccess(false);
		await aTimeout(0);
		expect(condition.permitted).to.be.false;
	});
});
