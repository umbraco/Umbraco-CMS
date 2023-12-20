import { UmbBlockTypeBase } from '../../types.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

export class UmbBlockTypeInputContext<BlockType extends UmbBlockTypeBase = UmbBlockTypeBase> extends UmbBaseController {
	#types = new UmbArrayState<BlockType>([], (type) => type.contentElementTypeKey);
	types = this.#types.asObservable();

	#blockTypeWorkspaceModalRegistration;

	constructor(host: UmbControllerHostElement, onWorkspaceRoutePathChanged: (routePath: string) => void) {
		super(host, 'blockTypeInput');

		this.#blockTypeWorkspaceModalRegistration = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('block-type')
			.onSetup(() => {
				return { data: { entityType: 'block-type', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				onWorkspaceRoutePathChanged(routeBuilder({}));
			});
	}

	create() {
		//TODO: make flow of picking a element type first, and then:
		this.#blockTypeWorkspaceModalRegistration.open({}, 'create/element-type-key');
		// TODO: Move to on submit:
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	requestRemoveItem(contentTypeKey: string) {
		alert('request remove ' + contentTypeKey);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}
}
