import type { UmbMenuItemActionApi, UmbMenuItemActionApiArgs } from './types.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

export abstract class UmbMenuItemActionApiBase<ArgsMetaType = never>
	extends UmbActionBase<UmbMenuItemActionApiArgs<ArgsMetaType>>
	implements UmbMenuItemActionApi<ArgsMetaType>
{
	public execute(): Promise<void> {
		return Promise.resolve();
	}
}

export class UmbActionMenuItemApi extends UmbMenuItemActionApiBase {
	override async execute() {}
}

export default UmbActionMenuItemApi;
