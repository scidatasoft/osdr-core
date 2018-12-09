using MassTransit;
using Newtonsoft.Json;
using Sds.Osdr.MachineLearning.Domain.Commands;
using Sds.Osdr.MachineLearning.Domain.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Osdr.IntegrationTests.Moq
{
    public partial class MachineLearning : IConsumer<PredictStructure>
    {
        public async Task Consume(ConsumeContext<PredictStructure> context)
        {
            var begin = DateTime.UtcNow;
            var resultHeader = $@"{{'predictionElapsedTime': 0 }}";


            var modelJson = $@"{{
                    'id': 'cca570be-e95e-47c7-85aa-41b0f055ce93',
                    'predictionElapsedTime': 0,
                    'trainingParameters': {{
                      'method': 'Naive Bayes',
                      'fingerprints': [
                        {{
                          'type': 'DESC'
                        }},
                        {{
                          'type': 'FCFP',
                          'radius': 3,
                          'size': 512
                        }}
                      ],
                      'name': 'model`s name',
                      'scaler': 'Still don`t know what is scaler',
                      'kFold': 2,
                      'testDatasetSize': 0.1,
                      'subSampleSize': 1,
                      'className': 'Soluble',
                      'consensusWeight': 125.61,
                      'modi': 0.2
                    }},
                    'applicabilityDomain': {{
                      'distance': 'distance',
                      'density': 'density'
                    }},
                    'property': {{
                      'code': 'some property code',
                      'category': 'toxicity',
                      'name': 'LC50',
                      'units': 'mg/L',
                      'description': 'LC50 description'
                    }},
                    'dataset': {{
                      'title': 'Dataset title',
                      'description': 'Dataset description'
                    }},
                    'result': {{
                      'value': 3.28,
                      'error': 1.341
                    }}
            }}";

            dynamic response = new ExpandoObject();
            var models = new List<dynamic>();
            var valueObj = JsonConvert.DeserializeObject<dynamic>(resultHeader);
            response.predictionElapsedTime = 0;

            for (int i = 0; i < context.Message.Models.ToList().Count(); i++)
            {
                var modelObj = JsonConvert.DeserializeObject<dynamic>(modelJson);
                var modelId = new Guid(context.Message.Models.ElementAt(i)["Id"].ToString());
                modelObj["id"] = modelId;
                modelObj["predictionElapsedTime"] = DateTime.UtcNow.Subtract(begin).TotalMilliseconds;
                models.Add(modelObj);
            }
            response.predictionElapsedTime = DateTime.UtcNow.Subtract(begin).TotalMilliseconds;

            response.models = models;

            await context.Publish<PredictedResultReady>(new
            {
                Id = context.Message.Id,
                Data = response
            });
        }
    }
}
