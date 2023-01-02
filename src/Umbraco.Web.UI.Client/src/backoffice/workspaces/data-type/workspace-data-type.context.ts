import { UmbWorkspaceNodeContext } from "../shared/workspace-context/workspace-node.context";
import type { UmbDataTypeStore, UmbDataTypeStoreItemType } from "@umbraco-cms/stores/data-type/data-type.store";
import type { DataTypeDetails } from "@umbraco-cms/models";

const DefaultDataTypeData = ({
	key: '',
	name: '',
	icon: '',
	type: 'dataType',
	hasChildren: false,
	parentKey: '',
	propertyEditorModelAlias: '',
	propertyEditorUIAlias: '',
	data: [],
}) as UmbDataTypeStoreItemType;

export class UmbWorkspaceDataTypeContext extends UmbWorkspaceNodeContext<UmbDataTypeStoreItemType, UmbDataTypeStore> {

	constructor(target:HTMLElement, entityKey: string) {
		super(target, DefaultDataTypeData, 'umbDataTypeStore', entityKey, 'dataType');
	}

	public setPropertyValue(propertyAlias: string, value: any) {
		// TODO: what if this is a tree item?
		const data = this._data.getValue();
		const property = (data as DataTypeDetails).data?.find((p) => p.alias === propertyAlias);
		if (!property) return;

		property.value = value;
		this._data.next({ ...data });
	}

}


