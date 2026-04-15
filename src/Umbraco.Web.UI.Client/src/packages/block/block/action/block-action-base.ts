import type { UmbBlockActionArgs } from './types.js';
import type { UmbBlockAction } from './block-action.interface.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';

/**
 * Base class for block action API implementations.
 * Provides default no-op implementations of getHref(), execute(), and getValidationDataPath().
 * Subclasses override only the methods they need.
 */
export abstract class UmbBlockActionBase<ArgsMetaType>
	extends UmbActionBase<UmbBlockActionArgs<ArgsMetaType>>
	implements UmbBlockAction<ArgsMetaType>
{
	public getHref(): Promise<string | undefined> {
		return Promise.resolve(undefined);
	}

	public execute(): Promise<void> {
		return Promise.resolve();
	}

	public getValidationDataPath(): Promise<string | undefined> {
		return Promise.resolve(undefined);
	}
}
