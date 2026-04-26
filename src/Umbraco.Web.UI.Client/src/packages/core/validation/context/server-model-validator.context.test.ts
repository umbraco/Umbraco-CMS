import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbApiError } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbServerModelValidatorContext } from './server-model-validator.context.js';
import { UmbValidationContext } from './validation.context.js';
import type { UmbValidationPathTranslator } from '../controllers/validation-path-translation/types.js';
import { UmbDataPathPropertyValueQuery } from '../utils/data-path-property-value-query.function.js';
import { umbScopeMapperForJsonPaths } from '../utils/scope-mapper-json-paths.function.js';
import { umbQueryMapperForJsonPaths } from '../utils/query-mapper-json-paths.function.js';

@customElement('umb-test-server-model-validator-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

function rejectedRequest(errors: Record<string, string[]>): Promise<UmbDataSourceResponse<string>> {
	const problemDetails = {
		type: 'Error',
		title: 'Validation failed',
		status: 400,
		operationStatus: 'PropertyValidationError',
		errors,
	};
	const error = new UmbApiError('Validation failed', 400, undefined, problemDetails);
	return Promise.resolve({ error } as unknown as UmbDataSourceResponse<string>);
}

describe('UmbServerModelValidatorContext', () => {
	let host: UmbTestControllerHostElement;
	let validationContext: UmbValidationContext;
	let serverValidator: UmbServerModelValidatorContext;

	beforeEach(async () => {
		host = new UmbTestControllerHostElement();
		document.body.appendChild(host);
		validationContext = new UmbValidationContext(host);
		serverValidator = new UmbServerModelValidatorContext(host);
		// Allow the consumeContext promise inside UmbServerModelValidatorContext to resolve.
		await Promise.resolve();
	});

	afterEach(() => {
		host.destroy();
		host.remove();
	});

	describe('askServerForValidation', () => {
		it('translates a single-level server error into a server-typed validation message', async () => {
			const data = {
				values: [{ alias: 'headline', culture: null, segment: null, value: 'x' }],
			};

			class HeadlineTranslator extends UmbControllerBase implements UmbValidationPathTranslator<typeof data> {
				async translate(paths: Array<string>, d: typeof data): Promise<Array<string>> {
					return await umbScopeMapperForJsonPaths(paths, '$.values', async (scoped) =>
						umbQueryMapperForJsonPaths(scoped, d.values, (entry) => UmbDataPathPropertyValueQuery(entry)),
					);
				}
			}

			serverValidator.addPathTranslator(HeadlineTranslator);

			await serverValidator.askServerForValidation(data, rejectedRequest({ '$.values[0].value': ['#err'] }));

			expect(serverValidator.isValid).to.be.false;

			const messages = validationContext.messages.getMessages();
			expect(messages.length).to.equal(1);
			expect(messages[0].type).to.equal('server');
			expect(messages[0].body).to.equal('#err');
			expect(messages[0].path).to.equal(
				`$.values[?(@.alias == 'headline' && @.culture == null && @.segment == null)].value`,
			);
		});

		// Bare-minimum repro for the multi-AND-filter lookup bug.
		// Why: the server reports `$.values[1].value.contentData[0].label` for the bs variant.
		// Outer query is built correctly as `?(@.alias == 'blocks' && @.culture == 'bs' && @.segment == null)`,
		// but `_GetNextArrayEntryFromPath` (json-path.function.ts) only applies `jsFilter[0]` (i.e. the alias check)
		// when looking the entry back up for the inner mapper, so it returns the en-US entry and the inner
		// `contentData[0]` resolves to `EN-KEY` instead of `BS-KEY`.
		it('resolves the correct inner entry when the outer query has multiple AND-conditions', async () => {
			const data = {
				values: [
					{
						alias: 'blocks',
						culture: 'en-US',
						segment: null,
						value: { contentData: [{ key: 'EN-KEY', label: 'en' }] },
					},
					{
						alias: 'blocks',
						culture: 'bs',
						segment: null,
						value: { contentData: [{ key: 'BS-KEY', label: 'bs' }] },
					},
				],
			};

			class TwoLevelTranslator extends UmbControllerBase implements UmbValidationPathTranslator<typeof data> {
				async translate(paths: Array<string>, d: typeof data): Promise<Array<string>> {
					return await umbScopeMapperForJsonPaths(paths, '$.values', async (scoped) =>
						umbQueryMapperForJsonPaths(
							scoped,
							d.values,
							(entry) => UmbDataPathPropertyValueQuery(entry),
							async (innerPaths, entry) => {
								if (!entry) return innerPaths;
								return await umbScopeMapperForJsonPaths(innerPaths, '$.value.contentData', async (cdPaths) =>
									umbQueryMapperForJsonPaths(cdPaths, entry.value.contentData, (block) => `?(@.key == '${block.key}')`),
								);
							},
						),
					);
				}
			}

			serverValidator.addPathTranslator(TwoLevelTranslator);

			await serverValidator.askServerForValidation(
				data,
				rejectedRequest({ '$.values[1].value.contentData[0].label': ['#err'] }),
			);

			const messages = validationContext.messages.getMessages();
			expect(messages.length).to.equal(1);
			expect(messages[0].path).to.equal(
				`$.values[?(@.alias == 'blocks' && @.culture == 'bs' && @.segment == null)].value.contentData[?(@.key == 'BS-KEY')].label`,
			);
		});
	});
});
