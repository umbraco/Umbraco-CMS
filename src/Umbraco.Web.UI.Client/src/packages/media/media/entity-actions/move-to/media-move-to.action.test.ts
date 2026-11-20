import { api as UmbMediaMoveToEntityAction } from './media-move-to.action.js';
import { UmbMediaItemRepository } from '../../repository/index.js';
import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbMediaTypeDetailRepository, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';

@customElement('umb-test-media-move-to-host')
class UmbTestHostElement extends UmbElementMixin(HTMLElement) {
	provideRootAccess(hasMediaRootAccess: boolean | undefined) {
		this.provideContext(UMB_CURRENT_USER_CONTEXT, {
			getHostElement: () => this,
			getHasMediaRootAccess: () => hasMediaRootAccess,
		} as never);
	}
}

type PickableFilter = (item: UmbTreeItemModel) => boolean;
type ActionWithFilter = { _getPickableFilter(unique: string): Promise<PickableFilter | undefined> };

describe('UmbMediaMoveToEntityAction', () => {
	const originals = {
		requestItems: UmbMediaItemRepository.prototype.requestItems,
		requestAllowedParentsOf: UmbMediaTypeStructureRepository.prototype.requestAllowedParentsOf,
		requestByUnique: UmbMediaTypeDetailRepository.prototype.requestByUnique,
	};

	let host: UmbTestHostElement;

	async function getRootPickable(options: { hasMediaRootAccess: boolean | undefined; allowedAtRoot: boolean }) {
		UmbMediaItemRepository.prototype.requestItems = (async () => ({
			data: [{ unique: 'moved-item', mediaType: { unique: 'image-type' } }],
		})) as never;
		UmbMediaTypeStructureRepository.prototype.requestAllowedParentsOf = (async () => ({ data: [] })) as never;
		UmbMediaTypeDetailRepository.prototype.requestByUnique = (async () => ({
			data: { allowedAtRoot: options.allowedAtRoot },
		})) as never;

		host.provideRootAccess(options.hasMediaRootAccess);

		const action = new UmbMediaMoveToEntityAction(host, {
			unique: 'moved-item',
			entityType: UMB_MEDIA_ENTITY_TYPE,
			meta: {} as never,
		});

		const filter = await (action as unknown as ActionWithFilter)._getPickableFilter('moved-item');
		const result = filter!({ unique: null, entityType: UMB_MEDIA_ROOT_ENTITY_TYPE } as UmbTreeItemModel);
		action.destroy();
		return result;
	}

	beforeEach(() => {
		host = new UmbTestHostElement();
		document.body.appendChild(host);
	});

	afterEach(() => {
		document.body.innerHTML = '';
		UmbMediaItemRepository.prototype.requestItems = originals.requestItems;
		UmbMediaTypeStructureRepository.prototype.requestAllowedParentsOf = originals.requestAllowedParentsOf;
		UmbMediaTypeDetailRepository.prototype.requestByUnique = originals.requestByUnique;
	});

	it('offers the media root when the user has root access and the type is allowed at the root', async () => {
		expect(await getRootPickable({ hasMediaRootAccess: true, allowedAtRoot: true })).to.be.true;
	});

	it('does not offer the media root when the user has no root access', async () => {
		expect(await getRootPickable({ hasMediaRootAccess: false, allowedAtRoot: true })).to.be.false;
	});

	it('offers the media root while root access is not yet known', async () => {
		expect(await getRootPickable({ hasMediaRootAccess: undefined, allowedAtRoot: true })).to.be.true;
	});

	it('does not offer the media root when the type is not allowed at the root', async () => {
		expect(await getRootPickable({ hasMediaRootAccess: true, allowedAtRoot: false })).to.be.false;
	});
});
