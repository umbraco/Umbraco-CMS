import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeContainerMergedModel,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbHintController, UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { extractJsonQueryProps, type UmbValidationController } from '@umbraco-cms/backoffice/validation';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/*
 * @internal
 * @module UmbContentValidationToHintsManager
 * @description
 * This manager observes the content type structure and validation messages, converting them into hints.
 * It is designed to be used in a content workspace to provide real-time feedback on content validation.
 */
export class UmbContentValidationToHintsManager<
	ContentTypeDetailModelType extends UmbContentTypeModel = UmbContentTypeModel,
> extends UmbControllerBase {
	/*workspace.hints.addOne({
		unique: 'exampleHintFromToggleAction',
		path: ['Umb.WorkspaceView.Document.Edit'],
		text: 'Hi',
		color: 'invalid',
		weight: 100,
	});

	TODO:
	* Maintaine structural awareness of all Properties.
	* Observe validation messages for all Properties, and turn them into Hints as fitting.
	*/

	#hintedMsgs: Set<string> = new Set();

	#containers: Array<UmbPropertyTypeContainerMergedModel> = [];

	constructor(
		host: UmbControllerHost,
		structure: UmbContentTypeStructureManager<ContentTypeDetailModelType>,
		validation: UmbValidationController,
		hints: UmbHintController<UmbVariantHint>,
		hintsPathPrefix: Array<string> = ['Umb.WorkspaceView.Document.Edit'],
	) {
		super(host);

		this.observe(structure.contentTypeMergedContainers, (merged) => {
			this.#containers = merged;
		});

		this.observe(validation.messages.messagesOfPathAndDescendant('$.values'), (messages) => {
			messages.forEach((message) => {
				if (this.#hintedMsgs.has(message.key)) return;

				// Get the value between [ and ] of message.path:
				const query = getValueBetweenBrackets(message.path);
				if (!query) return;
				const queryProps = extractJsonQueryProps(query);

				const alias = queryProps.alias;
				const variantId = UmbVariantId.CreateFromPartial(queryProps);

				structure.getPropertyStructureByAlias(alias).then((property) => {
					if (!property) return;

					let path: Array<string> = [];
					if (property.container) {
						const container = this.#containers.find((c) => c.ids.includes(property.container!.id));
						if (container) {
							path = container.path;
						} else {
							throw new Error(
								`Could not find the declared container of id "${property.container.id}" for property with alias: "${property.alias}"`,
							);
						}
					}

					hints.addOne({
						unique: message.key,
						path: [...hintsPathPrefix, ...path],
						text: '!',
						/*label: message.body,*/
						color: 'invalid',
						weight: 1000,
						variantId,
					});
					this.#hintedMsgs.add(message.key);
				});
			});
			this.#hintedMsgs.forEach((key) => {
				if (!messages.some((msg) => msg.key === key)) {
					this.#hintedMsgs.delete(key);
					hints.removeOne(key);
				}
			});
		});
	}
}

/**
 *
 * @param path {string} The path string to extract the value from.
 */
function getValueBetweenBrackets(path: string): string | null {
	const start = path.indexOf('[');
	if (start === -1) return null;

	const end = path.indexOf(']', start + 1);
	if (end === -1) return null;

	return path.substring(start + 1, end);
}
