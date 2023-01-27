import { UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN } from "../media.detail.store";
import { UmbEntityWorkspaceManager } from "../../../shared/components/workspace/workspace-context/entity-manager-controller";
import { UmbWorkspaceContext } from "../../../shared/components/workspace/workspace-context/workspace-context";
import { UmbWorkspaceEntityContextInterface } from "../../../shared/components/workspace/workspace-context/workspace-entity-context.interface";
import type { MediaDetails } from "@umbraco-cms/models";

export class UmbWorkspaceMediaContext extends UmbWorkspaceContext implements UmbWorkspaceEntityContextInterface<MediaDetails | undefined> {


	#manager = new UmbEntityWorkspaceManager(this._host, 'media', UMB_MEDIA_DETAIL_STORE_CONTEXT_TOKEN);

	public readonly data = this.#manager.state.asObservable();
	public readonly name = this.#manager.state.getObservablePart((state) => state?.name);

	setName(name: string) {
		this.#manager.state.update({name: name})
	}
	getEntityType = this.#manager.getEntityType;
	getUnique = this.#manager.getEntityKey;
	getEntityKey = this.#manager.getEntityKey;
	getStore = this.#manager.getStore;
	getData = this.#manager.getData;
	load = this.#manager.load;
	create = this.#manager.create;
	save = this.#manager.save;
	destroy = this.#manager.destroy;

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceMediaContext');
	}
}
