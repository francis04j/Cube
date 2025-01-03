using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace CubeLogic.InputFilesTriggerFunction;


 public class InputFilesTriggerFn
    {
        private readonly ILogger<InputFilesTriggerFn> _logger;

        public InputFilesTriggerFn(ILogger<InputFilesTriggerFn> logger)
        {
            _logger = logger;
        }


        [Function(nameof(InputFilesTriggerFn))]
        [BlobOutput("test-output/output-{datetime}.txt")]
        public string Run(
            [BlobTrigger("test-client/{name}")] BlobClient triggerItem, //name is the file name that was created in that container(test-client)
            [BlobInput("test-client/input.csv")] Stream input, //trigger on blobs that starts with input
            [BlobInput("test-client/config.json")] Stream config, //trigger on blobs that starts with input
            FunctionContext context
            )
        {
            var logger = context.GetLogger(nameof(InputFilesTriggerFn));
            string blobName = triggerItem.Name;
            logger.LogInformation("Triggered Item = {blobName}", blobName);

            if (blobName == "input.csv")
            {
                // Read input.csv
                string inputContent;
                using (var reader = new StreamReader(input))
                {
                    inputContent = reader.ReadToEnd();
                }
                logger.LogInformation("Input Content: {inputContent}", inputContent);
            }
            else if (blobName == "config.json")
            {
                // Read config.json
                string configContent;
                using (var reader = new StreamReader(config))
                {
                    configContent = reader.ReadToEnd();
                }
                logger.LogInformation("Config Content: {configContent}", configContent);
            }
            else
            {
                logger.LogError("File name does not match input.csv or config.json");
                return null;
            }


            // Get the current date and time
            string datetime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            // Blob Output
            return "blob-output content";
        }
    }