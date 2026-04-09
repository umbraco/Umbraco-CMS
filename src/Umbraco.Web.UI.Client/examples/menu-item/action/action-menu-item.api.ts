import { UmbMenuItemActionApiBase } from '@umbraco-cms/backoffice/menu';

/**
 * Example menu item action API
 * This action will log "Hello world" to the console when executed.
 */
export class ExampleActionMenuItemApi extends UmbMenuItemActionApiBase<never> {
	/**
	 * This method is executed when the menu item is clicked
	 */
	override async execute() {
		console.log('Hello world');
	}
}

// Declare an `api` export so the Extension Registry can initialize this class
export { ExampleActionMenuItemApi as api };
